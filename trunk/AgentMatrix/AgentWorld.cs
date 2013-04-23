using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DawnClient;
using DawnOnline.AgentMatrix;
using DawnOnline.AgentMatrix.Brains;
using DawnOnline.AgentMatrix.Repository;
using DawnOnline.Simulation;
using DawnOnline.Simulation.Builders;
using DawnOnline.Simulation.Entities;
using Microsoft.Xna.Framework;
using SharedConstants;

namespace DawnOnline.AgentMatrix
{
    class AgentWorld
    {
        private readonly DawnOnline.Simulation.Environment _staticEnvironment;

        //private readonly Dictionary<int, int> _serverIdToClientIdMap = new Dictionary<int, int>();
        //private readonly Dictionary<int, int> _clientIdToServerIdMap = new Dictionary<int, int>();
        private readonly HashSet<int> _createQueueClientIds = new HashSet<int>();
        private readonly HashSet<int> _serverIds = new HashSet<int>(); 

        private Random _randomize = new Random();
        private int _minNrOfSpawnPoints = 5;
        public int NrOfSpawnPointsReplicated { get; private set; }

        public AgentWorld(int instanceId)
        {
            _staticEnvironment = SimulationFactory.CreateEnvironment(instanceId);
        }

        public ReadOnlyCollection<IEntity> GetEntities()
        {
            var result = new List<IEntity>(_staticEnvironment.GetCreatures());
            result.AddRange(_staticEnvironment.GetObstacles());
            return result.AsReadOnly();
        }

        public ReadOnlyCollection<ICreature> GetCreatures()
        {
            var result = new List<ICreature>(_staticEnvironment.GetCreatures());
            return result.AsReadOnly();
        }

        internal void ProcessMyCreateRequests(List<int> creatureIds)
        {
            // creatureIds = id's of creatures that are created on request of this AgentWorld
            foreach (var id in creatureIds)
            {
                // Check already created
                if (_serverIds.Contains(id))
                {
                    // Exists already
                    continue;
                }

                // TODO: VERIFY, TEST, IMPROVE!!!! 
                // Check created requested
                if (_createQueueClientIds.Contains(id))
                {
                    Console.WriteLine("ProcessMyCreateRequests: " + _createQueueClientIds.First());

                    // Create mapping
                    _serverIds.Add(id);

                    // Remove from queue
                    _createQueueClientIds.Remove(id);
                }
            }
        }

        public void UpdateFromServer(ReadOnlyCollection<DawnClientEntity> entities)
        {
            // Add + update
            foreach (var entity in entities)
            {
                // Ignore bullets & rockets
                if (entity.Specy == EntityType.Bullet || entity.Specy == EntityType.Rocket)
                    continue;

                // Entities
                var position = new Vector2(entity.PlaceX, entity.PlaceY);
                IEntity myEntity = GetOrCreateWorldEntity(entity, position);

                // Update
                if (myEntity != null)
                {
                    CloneBuilder.UpdatePosition(myEntity, position, entity.Angle);
                    CloneBuilder.UpdateStatus(myEntity, entity.DamagePercent, entity.FatiguePercent, entity.ResourcePercent, entity.Score);
                }
            }
        }

        internal void ApplyDeleteFromServer(HashSet<int> destroyQueue)
        {
            foreach (var serverId in destroyQueue)
            {
                if (_serverIds.Contains(serverId))
                {
                    _staticEnvironment.RemoveCreature(serverId);
                }
            }
        }

        private IEntity GetOrCreateWorldEntity(DawnClientEntity entity, Vector2 position)
        {
            IEntity myEntity;
            if (!_serverIds.Contains(entity.Id))
            {
                // Add new with existing id!
                myEntity = CreateWorldEntity(entity, position);

                // This entity exists on the server
                if (myEntity != null)
                {
                    _serverIds.Add(entity.Id);
                }
            }
            else
            {
                // We already have this entity
                myEntity = FindWorldEntity(entity.Specy, entity.Id);
                Debug.Assert(myEntity != null);
            }
            return myEntity;
        }

        public int Think(double maxTime, List<int> creatureIds)
        {
            // Map server to clientIds
            return _staticEnvironment.Think(maxTime, new TimeSpan(SimulationConstants.UpdateIntervalOnServerInMs), creatureIds);
        }

        public void UpdateToServer(DawnClient.DawnClient dawnClient)
        {
            CreateNewCreaturesOnServer(dawnClient);
            SendActionsToServer(dawnClient);
            ClearActionQueues();
        }

        public void DoPhysics()
        {
            _staticEnvironment.UpdatePhysics(SimulationConstants.UpdateIntervalOnServerInMs);
        }

        private void CreateNewCreaturesOnServer(DawnClient.DawnClient dawnClient)
        {
            var creatures = _staticEnvironment.GetCreatures();
            foreach (var creature in creatures)
            {
                // Check in _createQueue = creation is already requested!
                if (_createQueueClientIds.Contains(creature.Id))
                    continue;

                // check if creature exists on server (and is returned to client)
                if (_serverIds.Contains(creature.Id))
                    continue;

                // Send spawnPoint info
                int spawnPointServerId = 0;
                if (creature.SpawnPoint != null && creature.SpawnPoint != creature) // (spawnPoint is also regarded as it's own spawnPoint, for family sake)
                {
                    if (!_serverIds.Contains(creature.SpawnPoint.Id))
                    {
                        // SpawnPoint is not known on server => wait untill it is.
                        continue;
                    }

                    spawnPointServerId = creature.SpawnPoint.Id;
                }

                _createQueueClientIds.Add(creature.Id);

                // Request creation on server
                // TODO: dawnClient.RequestCreatureCreationOnServer => this is a direct server request!! Should be a posponed command.
                Console.WriteLine("CreateNewCreaturesOnServer: " + creature.Id);
                dawnClient.RequestCreatureCreationOnServer(creature.Specy, creature.Place.Position.X, creature.Place.Position.Y, creature.Place.Angle, spawnPointServerId, creature.Id);
            }
        }

        private void SendActionsToServer(DawnClient.DawnClient dawnClient)
        {
            var creatures = _staticEnvironment.GetCreatures();

            foreach (var creature in creatures)
            {
                if (!_serverIds.Contains(creature.Id))
                {
                    // Creatures doesn't exist on server yet (or not returned by server)
                    // = wait a bit before sending commands
                    continue;
                }

                var actionQueue = creature.MyActionQueue;

                // Move
                if (actionQueue.ForwardThrustPercent > 0.0)
                {
                    if (actionQueue.ForwardThrustPercent > 0.9)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Forward100);
                    else if (actionQueue.ForwardThrustPercent > 0.8)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Forward90);
                    else if (actionQueue.ForwardThrustPercent > 0.7)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Forward80);
                    else if (actionQueue.ForwardThrustPercent > 0.6)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Forward70);
                    else if (actionQueue.ForwardThrustPercent > 0.5)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Forward60);
                    else if (actionQueue.ForwardThrustPercent > 0.4)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Forward50);
                    else if (actionQueue.ForwardThrustPercent > 0.3)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Forward40);
                    else if (actionQueue.ForwardThrustPercent > 0.2)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Forward30);
                    else if (actionQueue.ForwardThrustPercent > 0.1)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Forward20);
                    else
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Forward10);
                }
                if (actionQueue.ForwardThrustPercent < 0.0)
                {
                    if (actionQueue.ForwardThrustPercent < -0.9)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Backward100);
                    else if (actionQueue.ForwardThrustPercent < -0.8)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Backward90);
                    else if (actionQueue.ForwardThrustPercent < -0.7)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Backward80);
                    else if (actionQueue.ForwardThrustPercent < -0.6)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Backward70);
                    else if (actionQueue.ForwardThrustPercent < -0.5)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Backward60);
                    else if (actionQueue.ForwardThrustPercent < -0.4)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Backward50);
                    else if (actionQueue.ForwardThrustPercent < -0.3)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Backward40);
                    else if (actionQueue.ForwardThrustPercent < -0.2)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Backward30);
                    else if (actionQueue.ForwardThrustPercent < -0.1)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Backward20);
                    else
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Backward10);
                }
                if (actionQueue.TurnPercent > 0.0)
                {
                    if (actionQueue.TurnPercent > 0.9)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Right100);
                    else if (actionQueue.TurnPercent > 0.8)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Right90);
                    else if (actionQueue.TurnPercent > 0.7)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Right80);
                    else if (actionQueue.TurnPercent > 0.6)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Right70);
                    else if (actionQueue.TurnPercent > 0.5)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Right60);
                    else if (actionQueue.TurnPercent > 0.4)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Right50);
                    else if (actionQueue.TurnPercent > 0.3)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Right40);
                    else if (actionQueue.TurnPercent > 0.2)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Right30);
                    else if (actionQueue.TurnPercent > 0.1)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Right20);
                    else
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Right10);
                }
                if (actionQueue.TurnPercent < 0.0)
                {
                    if (actionQueue.TurnPercent < -0.9)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Left100);
                    else if (actionQueue.TurnPercent < -0.8)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Left90);
                    else if (actionQueue.TurnPercent < -0.7)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Left80);
                    else if (actionQueue.TurnPercent < -0.6)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Left70);
                    else if (actionQueue.TurnPercent < -0.5)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Left60);
                    else if (actionQueue.TurnPercent < -0.4)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Left50);
                    else if (actionQueue.TurnPercent < -0.3)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Left40);
                    else if (actionQueue.TurnPercent < -0.2)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Left30);
                    else if (actionQueue.TurnPercent < -0.1)
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Left20);
                    else
                        dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Left10);
                }

                // Attack
                if (actionQueue.Attack)
                {
                    dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Attack);
                }
                if (actionQueue.Fire)
                {
                    dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Fire);
                }
                if (actionQueue.FireRocket)
                {
                    dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Fire);
                }

                // Spawn
                if (actionQueue.RegisterSpawn)
                {
                    dawnClient.SendEntityCommand(creature.Id, AvatarCommand.RegisterSpawn);
                }

                // Rest
                if (actionQueue.Rest)
                {
                    dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Rest);
                }
            }
        }

        public void ClearActionQueues()
        {
            var creatures = _staticEnvironment.GetCreatures();
            foreach (var creature in creatures)
            {
                creature.ClearActionQueue();
            }
        }

        private IEntity FindWorldEntity(EntityType specy, int id)
        {
            if (CloneBuilder.IsObstacle(specy))
                return _staticEnvironment.GetObstacles().FirstOrDefault(e => e.Id == id);
            if (CloneBuilder.IsCreature(specy))
                return _staticEnvironment.GetCreatures().FirstOrDefault(e => e.Id == id);

            throw new NotImplementedException();
        }

        private IEntity CreateWorldEntity(DawnClientEntity clientEntity, Vector2 position)
        {
            if (CloneBuilder.IsObstacle(clientEntity.Specy))
            {
                var entity = CloneBuilder.CreateObstacle(clientEntity.Id, clientEntity.Specy, WorldConstants.WallHeight, WorldConstants.WallWide);
                if (_staticEnvironment.AddObstacle(entity, position))
                    return entity;
                //throw new NotSupportedException("Should not happen!"); 
                return null;
            }
            if (CloneBuilder.IsCreature(clientEntity.Specy))
            {
                ICreature spawnPoint = null;

                // TODO: refactor
                // When we create a Creature with a SpawnPoint, make sure we already have the SpawnPoint first (object ref is needed)
                // => but, when the Creature is a SpawnPoint, the SpawnPoint ref will be set to the creature itself
                if (!clientEntity.IsSpawnPoint && clientEntity.SpawnPointId != 0)
                {
                    // The creature has a SpawnPoint, but the spawnPoint is not yet received from the server.
                    // = wait untill spawnPoint is synched
                    if (!_serverIds.Contains(clientEntity.SpawnPointId))
                        return null;

                    spawnPoint = _staticEnvironment.GetCreatures().FirstOrDefault(c => c.Id == clientEntity.SpawnPointId);
                }

                var entity = CloneBuilder.CreateCreature(clientEntity.Specy, spawnPoint, clientEntity.Id);
                if (_staticEnvironment.AddCreature(entity, position, clientEntity.Angle))
                    return entity;
                //throw new NotSupportedException("Should not happen!"); 
                return null;
            }

            throw new NotImplementedException();
        }

        internal void AddCreature(ICreature creature)
        {
            _staticEnvironment.AddCreature(creature, new Vector2(0, 0), 0, false);
        }

        internal void RepopulateWorld(List<int> myCreatureIds)
        {
            RepopulateWorld(myCreatureIds, EntityType.PredatorSpawnPoint);
            RepopulateWorld(myCreatureIds, EntityType.PredatorSpawnPoint2);
            RepopulateWorld(myCreatureIds, EntityType.RabbitSpawnPoint);
        }

        private void RepopulateWorld(List<int> myCreatureIds, EntityType spawnpointType)
        {
            // Make sure we always have enough spawnpoints
            //var spawnPoints = _staticEnvironment.GetCreatures(EntityType.SpawnPoint);
            var spawnPoints = CreatureRepository.GetRepository().GetSortedRelevantSpawnpoints(spawnpointType);

            // Count the amount of spawnpoints "at my command!"
            var countMySpawnPoints = spawnPoints.Count(spawnPoint => myCreatureIds.Contains(spawnPoint.Id));

            if (countMySpawnPoints < _minNrOfSpawnPoints)
            {
                var timer = new Stopwatch();
                timer.Start();
                ICreature bestspawnPoint, crossoverMate;

                if (spawnPoints.Count == 0)
                {
                    // Everybody is dead... create a new eve
                    bestspawnPoint = AgentCreatureBuilder.CreateCreature(spawnpointType);
                    crossoverMate = bestspawnPoint;
                }
                else
                {
                    // find best spawnpoint
                    bestspawnPoint = GetBestspawnPoint(spawnPoints);
                    crossoverMate = GetBestspawnPoint(spawnPoints);
                }

                // Replicate
                //AddSpawnPoints(EntityType.Predator, 1);
                var newSpawnPoint = bestspawnPoint.Replicate(crossoverMate);
                newSpawnPoint.Mutate();
                CreatureRepository.GetRepository().Add(newSpawnPoint);

                // Send correct X, Y position at creation time
                // Take a safety boundary!
                const int boundary = 20;
                var position = new Vector2
                                   {
                                       X = _randomize.Next((int)WorldConstants.MaxX - 2 * boundary) + boundary, 
                                       Y = _randomize.Next((int)WorldConstants.MaxY - 2 * boundary) + boundary
                                   };
                //var position = new Vector2 { X = 0, Y = 0 };

                _staticEnvironment.AddCreature(newSpawnPoint, position, 0, false);
                NrOfSpawnPointsReplicated++;

                timer.Stop();
                Console.WriteLine("Replicate.timer: " + timer.ElapsedMilliseconds);
            }
        }

        private static ICreature GetBestspawnPoint(IList<ICreature> sortedSpawnPoints)
        {
            // Absolute

            //ICreature bestspawnPoint = spawnPoints[0];
            //foreach (var spawnPoint in spawnPoints)
            //{
            //    if (spawnPoint.CharacterSheet.Score > bestspawnPoint.CharacterSheet.Score)
            //    {
            //        bestspawnPoint = spawnPoint;
            //    }
            //}
            //return bestspawnPoint;

            // By better chance
            var randomizer = new Random((int)DateTime.Now.Ticks);
            for (; ; )
            {
                foreach (var sp in sortedSpawnPoints)
                {
                    if (randomizer.Next(3) == 0)
                    {
                        Console.WriteLine("Score to replicate: " + sp.CharacterSheet.Score);
                        return sp;
                    }
                }
            }
        }
    }
}
