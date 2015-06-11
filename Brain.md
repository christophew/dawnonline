# Introduction #

The brain consists of a collection of neural networks:
  * a nr of behaviour neural networks
  * a behaviour controller who decides which behaviour is active


# Brain design #

![https://docs.google.com/drawings/d/1fHKhwBHa2Fk8tQwUc3xjB0KFyz63aYjOf399IMKT9Qg/pub?w=739&h=294&nonsense=dummy.jpg](https://docs.google.com/drawings/d/1fHKhwBHa2Fk8tQwUc3xjB0KFyz63aYjOf399IMKT9Qg/pub?w=739&h=294&nonsense=dummy.jpg)


The behaviour mode networks are all have an identical topology.
The behaviour controller slightly differences: it has the same amount of input nodes, but 3 specific output nodes, each favouring a different behaviour.

**Note that 'forage', 'attack' & 'deliver' are arbitrary names that are only valid for the initial brain. As the brain is free to evolve in whatever possible way, the purpose of each behaviour will also change over time.**


# Neural network #

Custom neural network:
  * 3 layers: input, hidden, output
  * A large part of the output nodes are only used as new input nodes for the next time-slice (to have a state flow)

## Input layer ##

In the current setup, each agent has 3 eye sensors, 1 nose, 1 bumper, 3 monitors (health, stamina & resources gathered) and 2 random generators.

The eye sensors are configured to look for:
  * Prey: do I see something I can kill?
  * Resources: do I see something I can gather?
  * Walls: do I see a wall?
  * Family: do I see a member of my family?
  * SpawnPoint: do I see my SpawnPoint?

This results into 25 input nodes (and an equal amount of state nodes).

## Hidden layer ##

The hidden layer consists of an equal amount of nodes as the input layer (= 2x the input nodes).

## Output layer for the behaviour modes ##

We only have 2 ouput nodes:
  * Turn
  * Thrust

But, the output layer also contains the 25 state output nodes.

= 27 output nodes

## Output layer for the behaviour controller ##

The behaviour controller uses the same amount of input & hidden nodes.
But, it uses 3 specific ouput nodes:
  * ForageWeight
  * DeliverWeight
  * AttackWeight

... of course, again combined with the 25 reinforcement nodes.