using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DawnClient;
using DawnOnline.Simulation;
using DawnOnline.Simulation.Builders;
using DawnOnline.Simulation.Entities;
using Microsoft.Xna.Framework;
using SharedConstants;

namespace AgentMatrix
{
    class AgentWorld
    {
        private readonly DawnOnline.Simulation.Environment _staticEnvironment = SimulationFactory.CreateEnvironment();

        private readonly Dictionary<int, int> _serverIdToClientIdMap = new Dictionary<int, int>();
        private readonly Dictionary<int, int> _clientIdToServerIdMap = new Dictionary<int, int>();
        private readonly HashSet<int> _createQueueClientIds = new HashSet<int>();

        //private Random _randomize = new Random();
        private int _minNrOfSpawnPoints = 5;
        private int _nrOfSpawnPointsReplicated = 0;



        public ReadOnlyCollection<IEntity> GetEntities()
        {
            var result = new List<IEntity>(_staticEnvironment.GetCreatures());
            result.AddRange(_staticEnvironment.GetObstacles());
            return result.AsReadOnly();
        }

        internal void ProcessMyCreateRequests(ReadOnlyCollection<DawnClient.DawnClient.ClientServerIdPair> creatureIds)
        {
            // creatureIds = id's of creatures that are created on request of this AgentWorld
            foreach (var id in creatureIds)
            {
                // Check already created
                if (_serverIdToClientIdMap.ContainsKey(id.ServerId))
                {
                    // Exists already
                    continue;
                }

                // TODO: VERIFY, TEST, IMPROVE!!!! 
                // Check created requested
                if (_createQueueClientIds.Contains(id.ClientId))
                {
                    Console.WriteLine("ProcessMyCreateRequests: " + _createQueueClientIds.First());

                    // Create mapping
                    _serverIdToClientIdMap.Add(id.ServerId, id.ClientId);
                    _clientIdToServerIdMap.Add(id.ClientId, id.ServerId);

                    // Remove from queue
                    _createQueueClientIds.Remove(id.ClientId);
                }
            }
        }

        public void UpdateFromServer(ReadOnlyCollection<DawnClientEntity> entities)
        {
            // Add + update
            foreach (var entity in entities)
            {
                // Entities
                if (entity.Specy == EntityType.Wall || entity.Specy == EntityType.Box ||
                    entity.Specy == EntityType.Predator || entity.Specy == EntityType.SpawnPoint)
                {
                    var position = new Vector2(entity.PlaceX, entity.PlaceY);
                    IEntity myEntity = GetOrCreateWorldEntity(entity, position);

                    // Update
                    if (myEntity != null)
                        CloneBuilder.UpdatePosition(myEntity, position, entity.Angle);
                }

                // Avatars
            }

            // Deletes are handled by events!

            //var allEntities = _staticEnvironment.GetCreatures();
            //foreach (var entity in allEntities)
            //{
            //    // Check if entity still exists on 
            //    if (entities.FirstOrDefault(e => e.Id == entity.Id) == null)
            //    {
                    
            //    }
            //}
        }

        internal void ApplyDeleteFromServer(HashSet<int> destroyQueue)
        {
            foreach (var serverId in destroyQueue)
            {
                if (_serverIdToClientIdMap.ContainsKey(serverId))
                {
                    var clientId = _serverIdToClientIdMap[serverId];
                    _staticEnvironment.RemoveCreature(clientId);
                }
            }
        }

        private IEntity GetOrCreateWorldEntity(DawnClientEntity entity, Vector2 position)
        {
            IEntity myEntity;
            if (!_serverIdToClientIdMap.ContainsKey(entity.Id))
            {
                // Add new with existing id!
                myEntity = CreateWorldEntity(entity, position);

                // This entity exists on the server
                if (myEntity != null)
                {
                    _serverIdToClientIdMap.Add(entity.Id, myEntity.Id);
                    _clientIdToServerIdMap.Add(myEntity.Id, entity.Id);
                }
            }
            else
            {
                // We already have this entity
                myEntity = FindWorldEntity(entity.Specy, _serverIdToClientIdMap[entity.Id]);
                Debug.Assert(myEntity != null);
            }
            return myEntity;
        }

        public void Think(ReadOnlyCollection<DawnClient.DawnClient.ClientServerIdPair> creatureIds)
        {
            // Map server to clientIds
            var clientIds = creatureIds.Select(c => c.ClientId).ToList();
            _staticEnvironment.Think(100, new TimeSpan(SimulationConstants.UpdateIntervalOnServerInMs), clientIds);
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
                if (_clientIdToServerIdMap.ContainsKey(creature.Id))
                    continue;

                // Send spawnPoint info
                int spawnPointServerId = 0;
                if (creature.SpawnPoint != null && creature.SpawnPoint != creature) // (spawnPoint is also regarded as it's own spawnPoint, for family sake)
                {
                    if (!_clientIdToServerIdMap.TryGetValue(creature.SpawnPoint.Id, out spawnPointServerId))
                    {
                        // SpawnPoint is not known on server => wait untill it is.
                        continue;
                    }
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
            var creatureIds = dawnClient.CreatureIds;
            var creatures = _staticEnvironment.GetCreatures();

            foreach (var creature in creatures)
            {
                int serverId;
                if (!_clientIdToServerIdMap.TryGetValue(creature.Id, out serverId))
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
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Forward100);
                    else if (actionQueue.ForwardThrustPercent > 0.8)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Forward90);
                    else if (actionQueue.ForwardThrustPercent > 0.7)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Forward80);
                    else if (actionQueue.ForwardThrustPercent > 0.6)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Forward70);
                    else if (actionQueue.ForwardThrustPercent > 0.5)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Forward60);
                    else if (actionQueue.ForwardThrustPercent > 0.4)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Forward50);
                    else if (actionQueue.ForwardThrustPercent > 0.3)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Forward40);
                    else if (actionQueue.ForwardThrustPercent > 0.2)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Forward30);
                    else if (actionQueue.ForwardThrustPercent > 0.1)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Forward20);
                    else
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Forward10);
                }
                if (actionQueue.ForwardThrustPercent < 0.0)
                {
                    if (actionQueue.ForwardThrustPercent < -0.9)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Backward100);
                    else if (actionQueue.ForwardThrustPercent < -0.8)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Backward90);
                    else if (actionQueue.ForwardThrustPercent < -0.7)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Backward80);
                    else if (actionQueue.ForwardThrustPercent < -0.6)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Backward70);
                    else if (actionQueue.ForwardThrustPercent < -0.5)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Backward60);
                    else if (actionQueue.ForwardThrustPercent < -0.4)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Backward50);
                    else if (actionQueue.ForwardThrustPercent < -0.3)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Backward40);
                    else if (actionQueue.ForwardThrustPercent < -0.2)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Backward30);
                    else if (actionQueue.ForwardThrustPercent < -0.1)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Backward20);
                    else
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Backward10);
                }
                if (actionQueue.TurnPercent > 0.0)
                {
                    if (actionQueue.TurnPercent > 0.9)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Right100);
                    else if (actionQueue.TurnPercent > 0.8)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Right90);
                    else if (actionQueue.TurnPercent > 0.7)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Right80);
                    else if (actionQueue.TurnPercent > 0.6)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Right70);
                    else if (actionQueue.TurnPercent > 0.5)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Right60);
                    else if (actionQueue.TurnPercent > 0.4)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Right50);
                    else if (actionQueue.TurnPercent > 0.3)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Right40);
                    else if (actionQueue.TurnPercent > 0.2)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Right30);
                    else if (actionQueue.TurnPercent > 0.1)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Right20);
                    else
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Right10);
                }
                if (actionQueue.TurnPercent < 0.0)
                {
                    if (actionQueue.TurnPercent < -0.9)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Left100);
                    else if (actionQueue.TurnPercent < -0.8)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Left90);
                    else if (actionQueue.TurnPercent < -0.7)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Left80);
                    else if (actionQueue.TurnPercent < -0.6)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Left70);
                    else if (actionQueue.TurnPercent < -0.5)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Left60);
                    else if (actionQueue.TurnPercent < -0.4)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Left50);
                    else if (actionQueue.TurnPercent < -0.3)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Left40);
                    else if (actionQueue.TurnPercent < -0.2)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Left30);
                    else if (actionQueue.TurnPercent < -0.1)
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Left20);
                    else
                        dawnClient.SendEntityCommand(serverId, AvatarCommand.Left10);
                }

                // Attack
                if (actionQueue.Attack)
                {
                    dawnClient.SendEntityCommand(serverId, AvatarCommand.Attack);
                }
                if (actionQueue.Fire)
                {
                    dawnClient.SendEntityCommand(serverId, AvatarCommand.Fire);
                }
                if (actionQueue.FireRocket)
                {
                    dawnClient.SendEntityCommand(serverId, AvatarCommand.Fire);
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
                // TODO: insert without collision check
                //throw new NotSupportedException("Should not happen!"); 
                return null;
            }
            if (CloneBuilder.IsCreature(clientEntity.Specy))
            {
                IEntity spawnPoint = null;
                if (clientEntity.SpawnPointId != 0)
                {
                    // The creature has a SpawnPoint, but the spawnPoint is not yet received from the server.
                    // = wait untill spawnPoint is synched
                    int localFamilyId = 0;
                    if (!_serverIdToClientIdMap.TryGetValue(clientEntity.SpawnPointId, out localFamilyId))
                        return null;

                    spawnPoint = _staticEnvironment.GetCreatures(EntityType.SpawnPoint).FirstOrDefault(c => c.Id == localFamilyId);
                }

                var entity = CloneBuilder.CreateCreature(clientEntity.Specy, spawnPoint);
                if (_staticEnvironment.AddCreature(entity, position, clientEntity.Angle))
                    return entity;
                // TODO: insert without collision check
                //throw new NotSupportedException("Should not happen!"); 
                return null;
            }

            throw new NotImplementedException();
        }

        internal void CreateCreature(EntityType entityType)
        {
            var creature = CreatureBuilder.CreateCreature(entityType);
            _staticEnvironment.AddCreature(creature, new Vector2(0, 0), 0, false);
        }

        internal void RepopulateWorld()
        {
            // Make sure we always have enough spawnpoints
            var spawnPoints = _staticEnvironment.GetCreatures(EntityType.SpawnPoint);
            if (_staticEnvironment.GetCreatures(EntityType.SpawnPoint).Count < _minNrOfSpawnPoints)
            {
                var timer = new Stopwatch();
                timer.Start();
                ICreature bestspawnPoint, crossoverMate;

                if (spawnPoints.Count == 0)
                {
                    bestspawnPoint = CreatureBuilder.CreateSpawnPoint(EntityType.Predator);
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

                // TODO: send correct X, Y position at creation time
                //var position = new Vector2 { X = _randomize.Next((int)MaxX), Y = _randomize.Next((int)MaxY) };
                var position = new Vector2 { X = 0, Y = 0 };

                _staticEnvironment.AddCreature(newSpawnPoint, position, 0, false);
                _nrOfSpawnPointsReplicated++;

                timer.Stop();
                Console.WriteLine("Replicate.timer: " + timer.ElapsedMilliseconds);
            }
        }

        private static ICreature GetBestspawnPoint(IList<ICreature> spawnPoints)
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
            var tempSpawnPoints = spawnPoints.OrderByDescending(sp => sp.CharacterSheet.Score);
            for (; ; )
            {
                foreach (var sp in tempSpawnPoints)
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
