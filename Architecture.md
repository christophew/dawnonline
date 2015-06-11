# Introduction #

The coherence between the different technical components:
  * DawnServer:
    * the authoritative server
    * the the socket server app
  * User clients:
    * UnityClient
    * MogreClient
    * ConsoleClient
    * MonitoringClient
    * ...
  * AgentMatrix:
    * the AI controller for a collection of SpawnPoint & Creatures


# Overview #

![https://docs.google.com/drawings/d/1XGgyeVJcLfCcG-q7bQSUm6C6E9LnM1F1Y7mVxayyRbk/pub?w=920&h=645&nonsense=dummy.png](https://docs.google.com/drawings/d/1XGgyeVJcLfCcG-q7bQSUm6C6E9LnM1F1Y7mVxayyRbk/pub?w=920&h=645&nonsense=dummy.png)

## Connectivity ##

The nodes communicate using Photon Socket Server technology.

## Scalability ##

The agent controllers (AgentMatrix nodes) are perfectly scalable. Just deploy more instances on different machines.

The socket server itself is capable of handling a lot more messages then currently required, but is in itself also available in a load balanced mode.

The first expected performance bottleneck is probably the physics engine. This problem will be handled when it is encountered.

## AgentMatrix ##

An AgentMatrix represents the AI controller of a collection of agents. It uses timeslicing to guarantee a stable response time, and bulk message transport to optimize the network load.

## Deployment ##

Each node in the overview above can be deployed on a separate machine.

It is advised to place the front-end on a different machine than the Server and AgentMatrixes.

### Scenario 1: dev setup ###

All nodes on the same physical instance.

![https://docs.google.com/drawings/d/1dPr8udRBBri2gZLi5xcGPGCYDQWmTELyoVcqG8v2jQ8/pub?w=924&h=331&dummy=dummy.png](https://docs.google.com/drawings/d/1dPr8udRBBri2gZLi5xcGPGCYDQWmTELyoVcqG8v2jQ8/pub?w=924&h=331&dummy=dummy.png)

### Scenario 2: small load ###

Pump of the nr of AgentMatrixes. Preferably one for each physical CPU.

The Unity client is a CPU eater. Take it out of the equation.

![https://docs.google.com/drawings/d/1CbvKpIWdBZDUdSImIxMNRxgoSXOdQvJD3LfHDraHFv0/pub?w=924&h=331&dummy=dummy.png](https://docs.google.com/drawings/d/1CbvKpIWdBZDUdSImIxMNRxgoSXOdQvJD3LfHDraHFv0/pub?w=924&h=331&dummy=dummy.png)