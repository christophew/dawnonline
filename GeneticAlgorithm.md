# The metaphor #

All creatures belonging to the same family are equal. They posses the same neural networks in their brain. Therefor, it is not the creatures themselves that evolve, it is the SpawnPoint that evolves.

Again we take the metaphor of a bee hive. The bees won't evolve, it will be the queen that evolves.

Topics:
  * Replication
  * Fitness function
  * Mutation
  * Cross-over
  * Repository


# Replication #

When do we add a new SpawnPoint to the environment?

Each AgentMatrix can control a fixed amount of SpawnPoints. When 1 SpawnPoint is destroyed, there is room for a new one.


# Fitness function #

Which are the best SpawnPoints in the population?

The driving force of the ecosystem is the foodchain. Enemies need to be killed for their resources, receives need to be gathered and delivered to the SpawnPoints.

One single criteria satisfies this driving force: **the amount of resources delivered to the SpawnPoint**.

This criteria is out fitness function.

# Mutation #

A SpawnPoint contains a template creature. This is the prototype for all creatures created by this SpawnPoint.

It is the neural brain of this template creature that is mutated.

Possible mutation operations:
  * Increase/decrease a threshold on a network node
  * Increase/decrease a multiplier on a network edge
  * Enable/disable a network edge entirely

# Cross-over #

Our brain design is particularly suitable for cross-over operations: we combine different behaviours (neural networks) of the 2 parent SpawnPoints.

Example:
  * we take the controller & attack behaviour of 1 parent
  * we take the deliver & forage behaviour from the other parent

# Repository #

To enlarge our population, the entire history of the world is stored on disk. Every SpawnPoint that ever lived is kept with its obtained fitness score.

When choosing the parents for a new SpawnPoint, we take a selection from the 100 youngest SpawnPoints in the repository.
