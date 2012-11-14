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

        public ReadOnlyCollection<IEntity> GetEntities()
        {
            var result = new List<IEntity>(_staticEnvironment.GetCreatures());
            result.AddRange(_staticEnvironment.GetObstacles());
            return result.AsReadOnly();
        }

        internal void ProcessMyCreateRequests(ReadOnlyCollection<int> creatureIds)
        {
            // creatureIds = id's of creatures that are created on request of this AgentWorld
            foreach (var id in creatureIds)
            {
                // Check already created
                if (_serverIdToClientIdMap.ContainsKey(id))
                {
                    continue;
                }

                // TODO: VERIFY, TEST, IMPROVE!!!! 
                // Check created requested
                if (_createQueueClientIds.Count > 0)
                {
                    // Create mapping
                    var queueId = _createQueueClientIds.First();
                    _serverIdToClientIdMap.Add(id, queueId);
                    _clientIdToServerIdMap.Add(queueId, id);

                    // Remove from queue
                    _createQueueClientIds.Remove(queueId);
                }
            }
        }

        public void UpdateFromServer(ReadOnlyCollection<DawnClientEntity> entities)
        {
            // Add + update
            foreach (var entity in entities)
            {
                // Obstacles
                if (entity.Specy == EntityType.Wall || entity.Specy == EntityType.Box ||
                    entity.Specy == EntityType.Predator)
                {
                    var position = new Vector2(entity.PlaceX, entity.PlaceY);
                    IEntity myEntity = GetOrCreateWorldEntity(entity, position);

                    // Update
                    if (myEntity != null)
                        CloneBuilder.UpdatePosition(myEntity, position, entity.Angle);
                }

                // Avatars
            }

            // TODO: delete
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

        public void Think(ReadOnlyCollection<int> creatureIds)
        {
            // Map server to clientIds
            var clientIds = new List<int>();
            foreach (var id in creatureIds)
            {
                int clientId;
                if (_serverIdToClientIdMap.TryGetValue(id, out clientId))
                    clientIds.Add(clientId);
            }

            _staticEnvironment.Think(100, new TimeSpan(SimulationConstants.UpdateIntervalOnServerInMs), clientIds);
        }

        public void UpdateToServer(DawnClient.DawnClient dawnClient)
        {
            CreateNewCreaturesOnServer(dawnClient);
            SendActionsToServer(dawnClient);
            ClearActionQueues();
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

                _createQueueClientIds.Add(creature.Id);

                // Request creation on server
                // TODO: send initial position!!!!!
                // TODO: dawnClient.RequestCreatureCreationOnServer => this is a direct server request!! Should be a posponed command.
                dawnClient.RequestCreatureCreationOnServer(creature.Specy, 1);
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

                if (actionQueue.ForwardThrustPercent > 0.0)
                {
                    dawnClient.SendEntityCommand(serverId, AvatarCommand.RunForward);
                }
                if (actionQueue.ForwardThrustPercent < 0.0)
                {
                    dawnClient.SendEntityCommand(serverId, AvatarCommand.RunBackward);
                }
                if (actionQueue.TurnPercent > 0.0)
                {
                    dawnClient.SendEntityCommand(serverId, AvatarCommand.TurnRight);
                }
                if (actionQueue.TurnPercent < 0.0)
                {
                    dawnClient.SendEntityCommand(serverId, AvatarCommand.TurnLeft);
                }

                //if (actionQueue.Attack)
                //{
                //    dawnClient.SendEntityCommand(serverId, AvatarCommand.Attack);
                //}
                //if (actionQueue.Fire)
                //{
                //    dawnClient.SendEntityCommand(serverId, AvatarCommand.Fire);
                //}
                //if (actionQueue.FireRocket)
                //{
                //    dawnClient.SendEntityCommand(serverId, AvatarCommand.Fire);
                //}
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
                var entity = CloneBuilder.CreateCreature(clientEntity.Id, clientEntity.Specy);
                if (_staticEnvironment.AddCreature(entity as ICreature, position, clientEntity.Angle))
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
    }
}
