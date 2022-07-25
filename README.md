# NEAT
---

[NEAT](https://en.wikipedia.org/wiki/Neuroevolution_of_augmenting_topologies) or [NeuroEvolution of Augmenting Topologies](https://en.wikipedia.org/wiki/Neuroevolution_of_augmenting_topologies) is an method for evolving artificial neural networks.
This repository is a C# libarary for a slightly modified algorithm of NEAt.

## Usage
First create a 'GenomeEnvironment' instance. to create it it needs the parameters:
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
Then create the population using the function from the environment created earlier 'environment.CreatePopulation()'. There are parameters for the function, these parameters are:
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
> ```
> Genome[] population = environment.CreatePopulation(150, 2, 1, 1, 1, 1, false, Genome.ActivationFunction.SharpTanh, Genome.ActivationFunction.SharpSigmoid);
> ```
