using System;
using System.Collections.Generic;
using System.Linq;

namespace NEAT
{
    public class GenomeEnvironment
    {
        /// <summary>
        /// The info for mutating a genome.
        /// </summary>
        public MutationInfo MutateInfo { get; set; }

        /// <summary>
        /// The number of generation for a specie to improve or it will be penalized.
        /// </summary>
        public int DropOfAge { get; set; }
        /// <summary>
        /// The threshhold where if 2 genomes have a distance less than the threshhold.
        /// they will be in thr same species.
        /// </summary>
        public double CompatibilityThreshhold { get; set; }
        /// <summary>
        /// Control the dynamics of the variables.
        /// </summary>
        public Dynamics Dynamics { get; set; }
        /// <summary>
        /// The amount of movement to the CompatibilityThreshhold to make it closer to the species number target.
        /// </summary>
        public double ThreshholdStep { get; set; }
        /// <summary>
        /// The number of best genomes that will get passed to the next generation directly.
        /// </summary>
        public bool EliteMember { get; set; }
        /// <summary>
        /// The amount of influence for exess and disjoint gens when calculating the distance between 2 genomes.
        /// </summary>
        public double ExecessDisjointStrenght { get; set; }
        /// <summary>
        /// The amount of influence for matching gens weights diffrence when calculating the distance between 2 genomes.
        /// </summary>
        public double MatchingWeightsStrenght { get; set; }

        public int CurrentGeneration { get; private set; }
        public int PopulationNumber { get; private set; }

        public Genome[] Population { get; set; }

        private List<Specie> species;

        

        /// <summary>
        /// Create new Environment for genomes.
        /// </summary>
        /// <param name="info">The info for mutating a genome.</param>
        /// <param name="dynamics">Control the dynamics of the variables.</param>
        /// <param name="dropOfAge">The number of generation for a specie to improve or it will be penalized.</param>
        /// <param name="compatibilityThreshhold">The threshhold where if 2 genomes have a distance less than the threshhold they will be in thr same species.</param>
        /// <param name="execessDisjointStrenght">The amount of influence for exess and disjoint gens when calculating the distance between 2 genomes.</param>
        /// <param name="matchingWeightsStrenght">The amount of influence for matching gens weights diffrence when calculating the distance between 2 genomes.</param>
        public GenomeEnvironment(MutationInfo info, Dynamics dynamics, int dropOfAge, double compatibilityThreshhold, double execessDisjointStrenght, double matchingWeightsStrenght)
        {
            MutateInfo = info;
            Dynamics = dynamics;
            DropOfAge = dropOfAge;
            CompatibilityThreshhold = compatibilityThreshhold;
            ThreshholdStep = 0.5;
            ExecessDisjointStrenght = execessDisjointStrenght;
            MatchingWeightsStrenght = matchingWeightsStrenght;
        }
        public GenomeEnvironment(GenomeEnviromentSaveFile saveFile)
        {
            MutateInfo = saveFile.MutateInfo;
            DropOfAge = saveFile.DropOfAge;
            CompatibilityThreshhold = saveFile.CompatibilityThreshhold;
            Dynamics = saveFile.Dynamics;
            ThreshholdStep = saveFile.ThreshholdStep;
            EliteMember = saveFile.EliteMember;
            ExecessDisjointStrenght = saveFile.ExecessDisjointStrenght;
            MatchingWeightsStrenght = saveFile.MatchingWeightsStrenght;
            CurrentGeneration = saveFile.CurrentGeneration;
            PopulationNumber = saveFile.Population.Length;

            Population = new Genome[saveFile.Population.Length];
            species = new List<Specie>(saveFile.Species.Count);
            for (int i = 0; i < Population.Length; i++)
            {
                Population[i] = new Genome(saveFile.Population[i]);
                for (int i2 = 0; i2 < saveFile.Species.Count; i2++)
                {
                    if (saveFile.Population[i] == saveFile.Species[i2].Representetive)
                    {
                        species.Add(new Specie(saveFile.Species[i2], Population[i]));
                        break;
                    }
                }
            }
        }

        public GenomeEnviromentSaveFile Save() => new GenomeEnviromentSaveFile(this, species);

        public Genome[] CreatePopulation(int populationNumber, int inputNum, int outputNum, double connectionsRange, double nodesRange, double initConnectionsPercent, bool enableRecurrent, Genome.ActivationFunction hiddenAct, Genome.ActivationFunction outputAct)
        {
            Random random = new Random();

            PopulationNumber = populationNumber;

            Population = new Genome[populationNumber];
            for (int i = 0; i < populationNumber; i++)
            {
                Population[i] = new Genome(inputNum, outputNum, connectionsRange, nodesRange, initConnectionsPercent, enableRecurrent, hiddenAct, outputAct, random);
            }

            species = new List<Specie>();
            for (int i = 0; i < Population.Length; i++)
            {
                bool found = false;

                for (int i2 = 0; i2 < species.Count; i2++)
                {
                    if (Genome.Distace(species[i2].Representetive, Population[i], ExecessDisjointStrenght, MatchingWeightsStrenght) < CompatibilityThreshhold)
                    {
                        species[i2].AddGenome(Population[i]);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    species.Add(new Specie(Population[i], CurrentGeneration));
                }
            }

            return Population;
        }
        public Genome[] NextGeneration()
        {
            Random random = new Random();

            Speciate();

            species.Sort();

            double globalAvg = GetSpeciesAverageSumFitness();
            double fracSum = 0;

            for (int i = 0; i < species.Count; i++)
            {
                species[i].CalculateOffsprings(globalAvg, CurrentGeneration - species[i].LastImproveGeneration >= DropOfAge, PopulationNumber);
                fracSum += species[i].DoubleOffsprings - species[i].Offsprings;
            }

            if (Dynamics.DynamicThreshhold)
                CompatibilityThreshhold += Function.Sign(species.Count - Dynamics.SpeciesSizeTarget) * ThreshholdStep;

            // i = 1 so at least there is one specie left.
            for (int i = 1; i < species.Count; i++)
            {
                if (species[i].Offsprings == 0)
                {
                    species.RemoveAt(i);
                    i--;
                }
            }

            //BalanceSpeciesOffsprings(fracSum, random);

            int index = 0;

            for (int i = 0; i < species.Count; i++)
            {
                species[i].GenerateOffsprings(random, Population, index, MutateInfo, Dynamics, CurrentGeneration);
                index += species[i].Offsprings;
            }

            if (index < PopulationNumber)
            {
                int popDiff = PopulationNumber - index;

                species[0].Offsprings = popDiff;
                species[0].GenerateOffsprings(random, Population, index, MutateInfo, Dynamics, CurrentGeneration);
            }

            if (EliteMember)
            {
                Genome elite = species[0].Genomes[0];

                for (int i = 0; i < species.Count; i++)
                {
                    for (int i2 = 0; i2 < species[i].Genomes.Count; i2++)
                    {
                        Genome genome = species[i].Genomes[i2];
                        if (genome.Fitness > elite.Fitness)
                        {
                            elite = genome;
                        }
                    }
                }

                Population[Population.Length - 1] = elite;
            }

            CurrentGeneration++;

            return Population;
        }
        private double GetSpeciesAverageSumFitness()
        {
            double sum = 0;

            for (int i = 0; i < species.Count; i++)
            {
                species[i].CalculateAverageFitness(CurrentGeneration);

                sum += species[i].AverageFitness;
            }

            return sum;
        }

        private void Speciate()
        {
            for (int i = 0; i < species.Count; i++)
            {
                species[i].Genomes.Clear();
                species[i].Fitness = 0;
                species[i].MaxFitness = 0;
            }

            for (int i = 0; i < PopulationNumber; i++)
            {
                Genome genome = Population[i];

                bool found = false;

                for (int i2 = 0; i2 < species.Count; i2++)
                {
                    if (genome == species[i2].Representetive)
                    {
                        species[i2].AddGenome(genome);
                        found = true;
                        break;
                    }

                    if (Genome.Distace(genome, species[i2].Representetive, ExecessDisjointStrenght, MatchingWeightsStrenght) <= CompatibilityThreshhold)
                    {
                        species[i2].AddGenome(genome);
                        found = true;
                        break;
                    }
                }

                if (!found)
                    species.Add(new Specie(genome, CurrentGeneration));
            }

            species.RemoveAll(x => x.Genomes.Count == 0);
        }
        private void BalanceSpeciesOffsprings(double fracSum, Random random)
        {
            double rand = random.NextDouble() * fracSum;
            double summation = 0;
            int loop = Round(fracSum);

            for (int i2 = 0; i2 < loop; i2++)
            {
                for (int i = 0; i < species.Count; i++)
                {
                    summation += species[i].DoubleOffsprings - species[i].Offsprings;
                    if (summation >= rand)
                    {
                        species[i].Offsprings++;
                        break;
                    }
                }
            }
        }
        private int Round(double num)
        {
            int intNum = (int)num;
            double frac = num - intNum;

            if (frac >= 0.5)
                return intNum + 1;
            else
                return intNum;

        }

        [Serializable]
        public class Specie : IComparable<Specie>
        {
            public List<Genome> Genomes { get; private set; }
            public Genome Representetive { get; private set; }

            public double PreFitness { get; set; }
            public double Fitness { get; set; }
            public double MaxFitness { get; set; }
            public double AverageFitness { get; set; }
            public double AverageFitnessPercent { get; set; }

            public int LastImproveGeneration { get; set; }

            public double DoubleOffsprings { get; set; }
            public int Offsprings { get; set; }

            public Specie(Genome representetive, int currentGeneration)
            {
                LastImproveGeneration = currentGeneration;

                Representetive = representetive;

                Genomes = new List<Genome>() { representetive };
                Fitness += representetive.Fitness;
                MaxFitness = representetive.Fitness;
            }
            public Specie(GenomeEnviromentSaveFile.SpecieSaveFile saveFile, Genome representive)
            {
                LastImproveGeneration = saveFile.LastImproveGeneration;
                PreFitness = saveFile.PreFitness;
                Genomes = new List<Genome>();

                Representetive = representive;
            }

            public void AddGenome(Genome genome)
            {
                Genomes.Add(genome);
                Fitness += genome.Fitness;

                if (genome.Fitness > MaxFitness)
                {
                    MaxFitness = genome.Fitness;
                }
            }
            public void CalculateAverageFitness(int currentGeneration)
            {
                if (Fitness > PreFitness)
                    LastImproveGeneration = currentGeneration;

                AverageFitness = Fitness / Genomes.Count;
                AverageFitnessPercent = AverageFitness / MaxFitness;

                PreFitness = Fitness;
            }
            public void CalculateOffsprings(double totalSpeciesAvg, bool ageDropPassed, int populationNum)
            {
                if (ageDropPassed)
                {
                    Offsprings = 0;
                    DoubleOffsprings = 0;

                    return;
                }

                DoubleOffsprings = AverageFitness / totalSpeciesAvg * populationNum;
                Offsprings = (int)DoubleOffsprings;
            }
            public void GenerateOffsprings(Random random, Genome[] genomes, int index, MutationInfo info, Dynamics dynamics, int currentGen)
            {
                double genPercent = Math.Min(1, currentGen / (double)dynamics.GenerationDynamic);

                for (int i = 0; i < Offsprings; i++)
                {

                    Genome partner1 = ChoosePartner(random);
                    Genome partner2 = ChoosePartner(random);

                    Genome baby = Genome.Crossover(partner1, partner2, random);

                    MutationInfo genomeInfo = MutationInfo.ConvertToDynamic(info, dynamics, genPercent, Math.Max(partner1.Fitness, partner2.Fitness), AverageFitnessPercent, MaxFitness);

                    baby.Mutate(genomeInfo, random);

                    genomes[index + i] = baby;
                }

                Representetive = genomes[index];
            }
            private Genome ChoosePartner(Random random)
            {
                double rand = random.NextDouble() * Fitness;
                double summation = 0;
                for (int i = 0; i < Genomes.Count; i++)
                {
                    summation += Genomes[i].Fitness;
                    if (summation > rand)
                    {
                        return Genomes[i];
                    }
                }
                return Genomes[0];
            }

            // here its reversed for the best being first at 0 index
            public int CompareTo(Specie other)
            {
                double diff = AverageFitness - other.AverageFitness;

                if (diff < 0)
                {
                    return 1;
                }
                else if (diff > 0)
                {
                    return -1;
                }

                return 0;
            }
        }
    }
    [Serializable]
    public struct Dynamics
    {
        /// <summary>
        /// When true the threshhold well be adjusted to fit the target number of specie (SpeciesSizeTarget)
        /// </summary>
        public bool DynamicThreshhold { get; set; }
        /// <summary>
        /// The number of species wanted to reach. (will have effect if DynamicThreshhold is true)
        /// </summary>
        public int SpeciesSizeTarget { get; set; }

        public DynamicValue DynamicConnectionWeightMutation { get; set; }
        public DynamicValue DynamicConnectionWeightOffset { get; set; }
        public DynamicValue DynamicConnectionWeightChange { get; set; }
        public DynamicValue DynamicConnectionEnable { get; set; }
        public DynamicValue DynamicNodeBiasMutation { get; set; }
        public DynamicValue DynamicNodeBiasOffset { get; set; }
        public DynamicValue DynamicNodeBiasChange { get; set; }
        public DynamicValue DynamicAddConnection { get; set; }
        public DynamicValue DynamicAddNode { get; set; }

        public int GenerationDynamic { get; set; }
    }
    [Serializable]
    public struct DynamicValue
    {
        public bool Enabled { get; set; }
        public double Bias { get; set; }
        public double LowLimit { get; set; }
        public double HighLimit { get; set; }

        /// <summary>
        /// Create a new Dynamic value for a mutation value. this struct for managing a mutation variable for genomes by thier fitness.
        /// <br>for Example: a high mutation rate can break a good genome but a low mutation rate well slow bad ganomes from evolving.</br>
        /// <br>so creating a dynamic mutation rate which well change by the genome fitness. high fitness will reduce the mutation rate</br>
        /// <br>and a low fitness with encrease the mutation rate fot that genome.</br>
        /// </summary>
        /// <param name="enabled">if the dynamics for the value is enabled.</param>
        /// <param name="bias">delay for the mutation change (moslty in range [-5, 1]).
        /// <br>Where 1 will result in a slow change for the genomes in the edges (best or worst) and fast change for the average genome</br>
        /// <br>and -5 will result in a fast change for the genomes in the edges and slow change in the average genome.</br>
        /// <br>0 is a linear change.</br>
        /// </param>
        /// <param name="lowLimit">The mutation value for the low genomes in the edge.</param>
        /// <param name="highLimit">The mutation value for the high genomes in the edge.</param>
        public DynamicValue(bool enabled, double bias, double lowLimit, double highLimit)
        {
            Enabled = enabled;
            Bias = bias;
            LowLimit = lowLimit;
            HighLimit = highLimit;
        }
    }

    [Serializable]
    public class GenomeEnviromentSaveFile
    {
        /// <summary>
        /// The info for mutating a genome.
        /// </summary>
        public MutationInfo MutateInfo { get; }

        /// <summary>
        /// The number of generation for a specie to improve or it will be penalized.
        /// </summary>
        public int DropOfAge { get; }
        /// <summary>
        /// The threshhold where if 2 genomes have a distance less than the threshhold.
        /// they will be in thr same species.
        /// </summary>
        public double CompatibilityThreshhold { get; }
        /// <summary>
        /// Control the dynamics of the variables.
        /// </summary>
        public Dynamics Dynamics { get; }
        /// <summary>
        /// The amount of movement to the CompatibilityThreshhold to make it closer to the species number target.
        /// </summary>
        public double ThreshholdStep { get; }
        /// <summary>
        /// The number of best genomes that will get passed to the next generation directly.
        /// </summary>
        public bool EliteMember { get; }
        /// <summary>
        /// The amount of influence for exess and disjoint gens when calculating the distance between 2 genomes.
        /// </summary>
        public double ExecessDisjointStrenght { get; }
        /// <summary>
        /// The amount of influence for matching gens weights diffrence when calculating the distance between 2 genomes.
        /// </summary>
        public double MatchingWeightsStrenght { get; }

        public int CurrentGeneration { get; }

        public GenomeSaveFile[] Population { get; }

        public List<SpecieSaveFile> Species { get; }

        public GenomeEnviromentSaveFile(GenomeEnvironment environment, List<GenomeEnvironment.Specie> species)
        {
            MutateInfo = environment.MutateInfo;
            DropOfAge = environment.DropOfAge;
            CompatibilityThreshhold = environment.CompatibilityThreshhold;
            Dynamics = environment.Dynamics;
            ThreshholdStep = environment.ThreshholdStep;
            EliteMember = environment.EliteMember;
            ExecessDisjointStrenght = environment.ExecessDisjointStrenght;
            MatchingWeightsStrenght = environment.MatchingWeightsStrenght;
            CurrentGeneration = environment.CurrentGeneration;

            Population = new GenomeSaveFile[environment.Population.Length];
            Species = new List<SpecieSaveFile>(species.Count);
            for (int i = 0; i < Population.Length; i++)
            {
                GenomeSaveFile genome = new GenomeSaveFile(environment.Population[i]);
                for (int i2 = 0; i2 < species.Count; i2++)
                {
                    if (species[i2].Representetive == environment.Population[i])
                    {
                        Species.Add(new SpecieSaveFile(species[i2], genome));
                        break;
                    }
                }

                Population[i] = genome;
            }
        }

        [Serializable]
        public class SpecieSaveFile
        {
            public GenomeSaveFile Representetive { get; private set; }

            public double PreFitness { get; set; }
            public int LastImproveGeneration { get; set; }

            public SpecieSaveFile(GenomeEnvironment.Specie specie, GenomeSaveFile representive)
            {
                Representetive = representive;

                PreFitness = specie.PreFitness;
                LastImproveGeneration = specie.LastImproveGeneration;
            }
        }
    }
}
