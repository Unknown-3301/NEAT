# NEAT
---

[NEAT](https://en.wikipedia.org/wiki/Neuroevolution_of_augmenting_topologies) or [NeuroEvolution of Augmenting Topologies](https://en.wikipedia.org/wiki/Neuroevolution_of_augmenting_topologies) is an method for evolving artificial neural networks.
This repository is a C# libarary for a slightly modified algorithm of NEAt.

## Usage
First create a 'GenomeEnvironment' instance. to create it it needs the parameters:
- **MutationInfo**: a struct that contains information about the mutation parameters for evolving artificial neural networks, these parameters are:
  - **AddConnectionChance**: represent the chance of adding a new connection when mutating, where 0 is 0% and 1 is 100%.
