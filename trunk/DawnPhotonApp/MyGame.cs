using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DawnGame;
using DawnOnline.Simulation.Entities;
using DawnPhotonApp;
using Lite.Messages;
using Microsoft.Xna.Framework;
using SharedConstants;

namespace MyApplication
{
    using Lite;
    using Operations;
    using Photon.SocketServer;

    public class MyGame : LiteGame
    {
        private static DawnWorld _dawnWorldInstance = new DawnWorld();
        private static WorldSyncState _worldSyncState = new WorldSyncState();

        private DateTime _lastUpdateTime = DateTime.Now;
        private Dictionary<int, Vector2> _previousEntities = new Dictionary<int, Vector2>();

        private void UpdateDawnWorld()
        {
            var now = DateTime.Now;
            long millisecondsSinceLastFrame = (long) (now - _lastUpdateTime).TotalMilliseconds;
            //millisecondsSinceLastFrame /= 2; // Time-diliation
            _lastUpdateTime = now;

            //Debug.WriteLine("ms: " + millisecondsSinceLastFrame);

            // NEW DESIGN: _dawnWorld only handles the physics
            //_dawnWorldInstance.ThinkAll(30, new TimeSpan(millisecondsSinceLastFrame));
            _dawnWorldInstance.ApplyMove(millisecondsSinceLastFrame);
            _dawnWorldInstance.UpdatePhysics(millisecondsSinceLastFrame);

            ClearQueueOfLinkDeads();
            //var avatars = _dawnWorldInstance.Environment.GetCreatures();
            //foreach (var avatar in avatars)
            //{
            //    avatar.ClearActionQueue();
            //}
        }

        private void ClearQueueOfLinkDeads()
        {
            foreach (var entity in _dawnWorldInstance.Environment.GetCreatures())
            {
                if (!_worldSyncState.IsActive(entity.Id))
                    entity.ClearActionQueue();
            }
        }

        private void SendAvatarUpdates()
        {
            // Broadcast changes
            {
                // 104 = compressed position update
                {
                    var positionData = new List<Hashtable>();

                    foreach (var entity in _dawnWorldInstance.Environment.GetCreatures())
                    {
                        if (entity.Specy == EntityType.Avatar)
                            positionData.Add(CreateEntityData(entity));
                    }

                    SendPositionsEvent(positionData);
                }
            }

            this.ExecutionFiber.Schedule(SendAvatarUpdates, 50);
        }

        private void SendDawnWorld()
        {
            // Broadcast changes
            {
                // 101 = WorldInformation
                {
                    var data = new Dictionary<byte, object>();
                    data[0] = _dawnWorldInstance.GetWorldInformation();
                    var eData = new EventData((byte)EventCode.WorldInfo, data);
                    var sendParameters = new SendParameters { Unreliable = true };
                    this.PublishEvent(eData, this.Actors, sendParameters);
                }

                var currentEntities = new Dictionary<int, Vector2>();

                // 104 = compressed position update
                {
                    var positionData = new List<Hashtable>();

                    foreach (var entity in _dawnWorldInstance.Environment.GetCreatures())
                    {
                        currentEntities.Add(entity.Id, entity.Place.Position);

                        // TEMP REMOVE OPTIMISATION: CREATURES CAN BE CREATED BY AGENT MATRIX
                        //// Do not send updates when the object hasn't moved
                        //Vector2 previousPosition;
                        //if (_previousEntities.TryGetValue(entity.Id, out previousPosition))
                        //{
                        //    if (entity.Place.Position == previousPosition)
                        //        continue;
                        //}

                        positionData.Add(CreateEntityData(entity));
                    }
                    foreach (var entity in _dawnWorldInstance.Environment.GetObstacles())
                    {
                        currentEntities.Add(entity.Id, entity.Place.Position);

                        // Do not send updates when the object hasn't moved
                        Vector2 previousPosition;
                        if (_previousEntities.TryGetValue(entity.Id, out previousPosition))
                        {
                            if (entity.Place.Position == previousPosition)
                                continue;
                        }

                        // Ignore walls,they are send on WorldLoad
                        //if (entity.Specy == EntityType.Wall)
                        //    continue;

                        positionData.Add(CreateEntityData(entity));
                    }
                    foreach (var entity in _dawnWorldInstance.Environment.GetBullets())
                    {
                        currentEntities.Add(entity.Id, entity.Place.Position);
                        positionData.Add(CreateEntityData(entity));
                    }

                    SendPositionsEvent(positionData);
                }

                // 103 = destroyed
                {
                    var killedHash = new Hashtable();

                    int index = 0;
                    foreach (var previousEntity in _previousEntities.Keys)
                    {
                        if (!currentEntities.ContainsKey(previousEntity))
                        {
                            // TODO: optimize second parameter
                            killedHash.Add(index++, previousEntity);
                        }
                    }

                    if (killedHash.Count > 0)
                    {
                        // Send killed reliable
                        var data = new Dictionary<byte, object>();
                        data[0] = killedHash;
                        var eData = new EventData((byte)EventCode.Destroyed, data);
                        var sendParameters = new SendParameters { Unreliable = false };
                        this.PublishEvent(eData, this.Actors, sendParameters);
                    }
                }

                _previousEntities = currentEntities;

                // Walls
                //SendWalls();
            }

            this.ExecutionFiber.Schedule(SendDawnWorld, 100);
        }

        private void SendPositionsEvent(List<Hashtable> positions)
        {
            // TODO: warnings when we exceed the max packet-size!

            var sendParameters = new SendParameters { Unreliable = true };

            // Split the list into fragments
            const int fragmentSize = 15;

            byte index = 0;
            var currentDataList = new Dictionary<byte, object>();
            foreach (var position in positions)
            {
                currentDataList.Add(index++, position);

                if (index > fragmentSize)
                {
                    var eData = new EventData((byte)EventCode.BulkPositionUpdate, currentDataList);
                    this.PublishEvent(eData, this.Actors, sendParameters);

                    index = 0;
                    currentDataList = new Dictionary<byte, object>();
                }
            }

            // Send remainder
            if (currentDataList.Count > 0)
            {
                var eData = new EventData((byte)EventCode.BulkPositionUpdate, currentDataList);
                this.PublishEvent(eData, this.Actors, sendParameters);
            }
        }

        private Hashtable CreateEntityData(IEntity entity)
        {
            var dawnEntity = new Hashtable();
            dawnEntity[0] = entity.Id;
            dawnEntity[1] = (byte)entity.Specy;
            dawnEntity[2] = entity.Place.Position.X;
            dawnEntity[3] = entity.Place.Position.Y;
            dawnEntity[4] = entity.Place.Angle;
            dawnEntity[5] = _worldSyncState.IsActive(entity.Id);

            var creature = entity as ICreature;
            if (creature != null && creature.SpawnPoint != null)
            {
                dawnEntity[6] = creature.SpawnPoint.Id;
            }
            return dawnEntity;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MyGame"/> class.
        /// </summary>
        /// <param name="gameName">The name of the game.</param>
        public MyGame(string gameName)
            : base(gameName)
        {

            this.ExecutionFiber.Schedule(SendDawnWorld, 1500);
            this.ExecutionFiber.Schedule(SendAvatarUpdates, 1500);
            this.ExecutionFiber.ScheduleOnInterval(UpdateDawnWorld, 1000, SimulationConstants.UpdateIntervalOnServerInMs);
        }


        /// <summary>
        /// Called for each operation in the execution queue.
        /// </summary>
        /// <param name="peer">The peer.</param>
        /// <param name="operationRequest">The operation request to execute.</param>
        /// <param name="sendParameters"></param>
        /// <remarks>
        /// ExecuteOperation is overriden to handle our custom operations.
        /// </remarks>
        protected override void ExecuteOperation(LitePeer peer, OperationRequest operationRequest, SendParameters sendParameters)
        {
            switch ((MyOperationCodes)operationRequest.OperationCode)
            {
                case MyOperationCodes.GameOperation:
                    this.HandleMyGameOperation(peer, operationRequest, sendParameters);
                    break;

                case MyOperationCodes.AvatarCommand:
                    {
                        HandleAvatorCommand(operationRequest);
                        break;
                    }

                case MyOperationCodes.BulkEntityCommand:
                    {
                        HandleBulkEntityCommand(operationRequest);
                        break;
                    }

                case MyOperationCodes.AddEntity:
                    {
                        var parameters = (Hashtable)operationRequest.Parameters[0];

                        // Add to world
                        var entityType = (EntityType)parameters[0];
                        var position = new Vector2((float) parameters[1], (float) parameters[2]);
                        var angle = (float) parameters[3];
                        var spawnPoint = (int) parameters[4];
                        var clientId = (int)parameters[5]; // client referenceId of created creature

                        var newCreature = _dawnWorldInstance.AddCreature(entityType, position, angle, spawnPoint);

                        // Send response
                        var eData = new Dictionary<byte, object>();
                        eData[0] = (newCreature != null) ? newCreature.Id : 0;
                        eData[1] = clientId; // client referenceId of created creature
                        var response = new OperationResponse((byte)MyOperationCodes.AddEntity, eData);
                        peer.SendOperationResponse(response, new SendParameters { Unreliable = false });

                        break;
                    }

                case MyOperationCodes.LoadWorld:
                    {
                        var avatar = _dawnWorldInstance.AddAvatar();
                        //var slowObjects = _dawnWorldInstance.Environment.GetObstacles().Where(o => o.Specy == EntityType.Wall).ToList();
                        var slowObjects = _dawnWorldInstance.Environment.GetObstacles().ToList();
                        var slowCreatures = _dawnWorldInstance.Environment.GetCreatures(EntityType.SpawnPoint);
                        slowObjects.AddRange(slowCreatures);
                        var slowObjectsParam = slowObjects.Select(CreateEntityData).ToArray();

                        // Send response
                        var eData = new Dictionary<byte, object>();
                        eData[0] = avatar.Id;
                        eData[1] = slowObjectsParam;
                        var response = new OperationResponse((byte)MyOperationCodes.LoadWorld, eData);
                        peer.SendOperationResponse(response, new SendParameters{Unreliable = false});

                        break;
                    }

                default:
                    // all other operations will be handled by the LiteGame implementation
                    base.ExecuteOperation(peer, operationRequest, sendParameters);
                    break;
            }
        }

        private static void HandleAvatorCommand(OperationRequest operationRequest)
        {
            var avatarId = (int) operationRequest.Parameters[1];
            var commandsParameter = (byte[]) operationRequest.Parameters[0];

            var commands = new List<AvatarCommand>();
            foreach (var byteCommand in commandsParameter)
            {
                commands.Add((AvatarCommand)byteCommand);
            }

            ApplyEntityCommands(avatarId, commands.Cast<AvatarCommand>().ToList());
        }

        private static void HandleBulkEntityCommand(OperationRequest operationRequest)
        {
            var entityCommands = operationRequest.Parameters.Values.Cast<Hashtable>();
            foreach (var entityCommand in entityCommands)
            {
                HandleEntityCommand(entityCommand);
            }
        }

        private static void HandleEntityCommand(Hashtable parameters)
        {
            var entityId = (int)parameters[0];
            var nrOfCommands = (byte)parameters[1];

            var commands = new List<AvatarCommand>();
            for (int i=0; i < nrOfCommands; i++)
            {
                commands.Add((AvatarCommand)parameters[2+i]);
            }

            ApplyEntityCommands(entityId, commands);
        }

        private static void ApplyEntityCommands(int entityId, List<AvatarCommand> commands)
        {
            var avatar = _dawnWorldInstance.GetAvatar(entityId);
            if (avatar == null)
            {
                // TODO: seperate CREATURE & AVATOR logic & commands?
                avatar = _dawnWorldInstance.GetCreature(entityId);
            }
            if (avatar == null)
            {
                //throw new NotImplementedException("TODO: client notifications");
                // Client not yet in synch with kill operation
                return;
            }

            // TEST: clear ActionQueue before update instead of on fixed intervals
            // = + seems better
            // = - if connection is lost => action queue is never cleared
            avatar.ClearActionQueue();

            foreach (var command in commands)
            {
                ApplyCreatureCommand.ApplyCommand(avatar, command);
            }

            _worldSyncState.EntityCommandReceived(entityId);
        }

        private void HandleMyGameOperation(LitePeer peer, OperationRequest operationRequest, SendParameters sendParameters)
        {
            var requestContract = new MyGameRequest(peer.Protocol, operationRequest);
            requestContract.OnStart();

            var responseContract = new MyGameResponse();
            responseContract.Response = "You are in game " + this.Name;

            var operationResponse = new OperationResponse(operationRequest.OperationCode, responseContract);
            peer.SendOperationResponse(operationResponse, sendParameters);

            requestContract.OnComplete();
        }
    }
}
