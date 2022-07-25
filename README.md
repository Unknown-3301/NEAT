# NEAT
---

[NEAT](https://en.wikipedia.org/wiki/Neuroevolution_of_augmenting_topologies) or [NeuroEvolution of Augmenting Topologies](https://en.wikipedia.org/wiki/Neuroevolution_of_augmenting_topologies) is an method for evolving artificial neural networks.
This repository is a C# libarary for a slightly modified algorithm of NEAt.

## Usage
First create a 'GenomeEnvironment' instance. to create it it needs the parameters:
- **MutationInfo**: a struct that contains information about the mutation parameters for evolving artificial neural networks, these parameters are:
  - **AddConnectionChance**: represent the chance of adding a new connection when mutating, where 0 is 0% and 1 is 100%.
  - **AddNodeChance**: represent the chance of adding a new node when mutating, where 0 is 0% and 1 is 100%.
  - **ConnectionEnableChance**: represent the chance of enabling a disabled connection when mutating, where 0 is 0% and 1 is 100%.
  - **ConnectionWeightChangeChance**: represent the change of choosing the first of two methods for mutating a connection. The first method is offsetting the weight of a connection by a random value in a certain range. The second method is generating a new weight for the connection, so if ConnectionWeightChangeChance is 0.9 for example that means there is 90% chance to choose the first method to mutate the connection, and 10% chance to choose the second method.
  - **ConnectionWeightMutationChance**: represent the chance of mutating a connection, where 0 is 0% and 1 is 100%.
  - **ConnectionWeightOffsetPower**: represent an approximate range for offsetting the weight of a connection when mutating it using the first method.
  - **NodeBiasChangeChance**: represent the change of choosing the first of two methods for mutating a node. It's the same as **ConnectionWeightChangeChance** but a node instead of connection, and the node bias instead of the connection weight, where 0 is 0% and 1 is 100%.
  - **NodeBiasMutationChance**:  represent the chance of mutating a node, where 0 is 0% and 1 is 100%.
  - **NodeBiasOffsetPower**: The same as **ConnectionWeightOffsetPower** but to the node bias instead of connection weight.
