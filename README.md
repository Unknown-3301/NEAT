# NEAT
---

[NEAT](https://en.wikipedia.org/wiki/Neuroevolution_of_augmenting_topologies) or [NeuroEvolution of Augmenting Topologies](https://en.wikipedia.org/wiki/Neuroevolution_of_augmenting_topologies) is an method for evolving artificial neural networks.
This repository is a C# library for a slightly modified algorithm of NEAT.

## Usage
First create a `GenomeEnvironment` instance. to create it it needs the parameters:
- **MutationInfo**: a struct that holds information about the mutation parameters for evolving artificial neural networks, these parameters are:
  - **AddConnectionChance**: represent the chance of adding a new connection when mutating, where 0 is 0% and 1 is 100%.
  - **AddNodeChance**: represent the chance of adding a new node when mutating, where 0 is 0% and 1 is 100%.
  - **ConnectionEnableChance**: represent the chance of enabling a disabled connection when mutating, where 0 is 0% and 1 is 100%.
  - **ConnectionWeightChangeChance**: represent the change of choosing the first of two methods for mutating a connection. The first method is offsetting the weight of a connection by a random value in a certain range. The second method is generating a new weight for the connection, so if ConnectionWeightChangeChance is 0.9 for example that means there is 90% chance to choose the first method to mutate the connection, and 10% chance to choose the second method.
  - **ConnectionWeightMutationChance**: represent the chance of mutating a connection, where 0 is 0% and 1 is 100%.
  - **ConnectionWeightOffsetPower**: represent an approximate range for offsetting the weight of a connection when mutating it using the first method.
  - **NodeBiasChangeChance**: represent the change of choosing the first of two methods for mutating a node. It's the same as **ConnectionWeightChangeChance** but a node instead of connection, and the node bias instead of the connection weight, where 0 is 0% and 1 is 100%.
  - **NodeBiasMutationChance**:  represent the chance of mutating a node, where 0 is 0% and 1 is 100%.
  - **NodeBiasOffsetPower**: The same as **ConnectionWeightOffsetPower** but to the node bias instead of connection weight.
  ```
  // as example
  MutationInfo info = new MutationInfo()
  {
      AddConnectionChance = 0.05,
      AddNodeChance = 0.02,
      ConnectionEnableChance = 0.1,
      ConnectionWeightChangeChance = 0.9,
      ConnectionWeightMutationChance = 0.3,
      ConnectionWeightOffsetPower = 1,
      NodeBiasChangeChance = 0.9,
      NodeBiasMutationChance = 0.3,
      NodeBiasOffsetPower = 1,
  };
  ```
 
- **Dynamics**: a struct that holds the information about how dynamic the mutation parameters and other parameters are. There is alot of details in this struct and it's parameters, so I will explain the basic parameters.
  - **DynamicThreshhold**: when true the **Threshold** value will be adjusted close to **SpeciesSizeTarget**. The **Threshold** value will be explained later.
  - **SpeciesSizeTarget**: the number of species wanted to get to, this value will be usefull only if **DynamicThreshhold** is true.
  ```
  // as example
  Dynamics dynamics = new Dynamics()
  {
      DynamicThreshhold = true,
      SpeciesSizeTarget = 32
  };
  ```

- **dropOfAge**: the number of generations for a specie to improve or it will be penalized.
- **CompatibilityThreshold**: the value to determine if two genomes can be in the same specie, so if we have two genomes and calculate how different they are from each other, by a distance function. We will compare the value resulted from the function with the threshold, if the value is smaller then they are similar and put them in the same specie.
- **execessDisjointStrength**: represent the amount of importance for execess and disjoint genes when calculating the distance between 2 genomes.
- **matchingWeightsStrength**: represent the amount of importance for the difference between the weights of the matching genes when calculating the distance between 2 genomes.
```
// as example
GenomeEnvironment environment = new GenomeEnvironment(info, dynamics, 15, 1, 1, 0.5);
environment.ThreshholdStep = 0.1;
```
Then create the population using the function from the environment created earlier `environment.CreatePopulation()`. There are parameters for the function, these parameters are:
- **populationNumber**: the number of genomes desired for the population.
- **inputNum**: the number of input nodes for each genome.
- **outputNum**: the number of output nodes for each genome.
- **connectionsRange**: The min and max range of the connections weight when creating them, so if it was for example 2 then the connection weights will be a random number between -2 and 2.
- **nodesRange**: same as **connectionsRange** but with node weights instead of connection weights.
- **initConnectionsPercent**: the percent of connection between input and output nodes when creating the genome, where 0 is 0% and 1 is 100%.
- **enableRecurrent**: determine whether the the network can contain recurrent net structure.
- **hiddenAct**: the activation function type for the output layer.
- **outputAct**: the activation function type for the hidden layer.
> note that almost all the random numbers are from a gaussian distribution.
```
Genome[] population = environment.CreatePopulation(150, 2, 1, 1, 1, 1, false, Genome.ActivationFunction.SharpTanh, Genome.ActivationFunction.SharpSigmoid);
```
Now evaluate the whole population using the Fitness property for every genome `genome.Fitness`, that property shows how good a genome is (like score). Here is an example for evaluation for the XOR problem.
```
for (int i = 0; i < population.Length; i++)
{
   TestGenome(population[i]);
}

void TestGenome(Genome genome)
{
   genome.Fitness = 0;
   double[] xorInput = new double[2]; //the genome input array, it's size must equal the inputNum (number of input nodes in the genome)
   
   //test the xor input (0, 0), the expected output is 0
   xorInput[0] = 0;
   xorInput[1] = 0;
   double genomeOutput = genome.CalculateOutput(xorInput)[0]; //after using the genome it outputs an array of doubles, it represent the output values from the output nodes in the genome. We only took the first element because the xor only outputs one number
   genome.Fitness += 1 - genomeOutput;
   
   //test the xor input (1, 0), the expected output is 1
   xorInput[0] = 1;
   xorInput[1] = 0;
   genomeOutput = genome.CalculateOutput(xorInput)[0];
   genome.Fitness += genomeOutput;
   
   //test the xor input (0, 1), the expected output is 1
   xorInput[0] = 0;
   xorInput[1] = 1;
   genomeOutput = genome.CalculateOutput(xorInput)[0];
   genome.Fitness += genomeOutput;
   
   //test the xor input (1, 1), the expected output is 0
   xorInput[0] = 1;
   xorInput[1] = 1;
   genomeOutput = genome.CalculateOutput(xorInput)[0];
   genome.Fitness += 1 - genomeOutput;
}
```
Now after evaluating the whole population we call `environment.NextGeneration()` that will return a array of genomes that represent the nex generationg genomes.
```
population = environment.NextGeneration();
```
Now keep evaluating and calling `environment.NextGeneration()` until one of the genome have the fitness desired.

## Saving and loading
if you want to save a genome you can use the function `genome.Save()`, it will return a **GenomeSaveFile** class, this class will hold the genome information, this class can be saved to computer file using serialization. When loading you can just create a new genome with this **GenomeSaveFile** to load 
```
GenomeSaveFile save = //get it from the computer;
Genome genome = new Genome(save);
```
