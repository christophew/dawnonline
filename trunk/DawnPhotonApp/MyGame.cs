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
using PerformanceMonitoring;
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
        private Dictionary<int, IEntityPhotonPacket> _previousPositions = new Dictionary<int, IEntityPhotonPacket>();
        private Dictionary<int, IEntityPhotonPacket> _previousStatuses = new Dictionary<int, IEntityPhotonPacket>();


        private int _intervalSendAvatarUpdates = 75;
        private int _intervalSendPositions = 100;
        private int _intervalSendStatus = 500;
        private const int _fragmentSize = 15;


        /// <summary>
        /// Initializes a new instance of the <see cref="MyGame"/> class.
        /// </summary>
        /// <param name="gameName">The name of the game.</param>
        public MyGame(string gameName)
            : base(gameName)
        {
            this.ExecutionFiber.Schedule(SendDawnWorld, 1500);
            this.ExecutionFiber.Schedule(SendAvatarUpdates, 1550);
            this.ExecutionFiber.Schedule(SendPositions, 1600);
            this.ExecutionFiber.ScheduleOnInterval(UpdateDawnWorld, 1000, SimulationConstants.UpdateIntervalOnServerInMs);
        }

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

            ClearActionQueueOfLinkDeads();
            //var avatars = _dawnWorldInstance.Environment.GetCreatures();
            //foreach (var avatar in avatars)
            //{
            //    avatar.ClearActionQueue();
            //}
        }

        private void ClearActionQueueOfLinkDeads()
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
                // 104 = BulkPositionUpdate
                {
                    var currentEntities = new Dictionary<int, IEntityPhotonPacket>();

                    foreach (var entity in _dawnWorldInstance.Environment.GetCreatures())
                    {
                        if (entity.Specy == EntityType.Avatar)
                            currentEntities.Add(entity.Id, CreateEntityPosition(entity));
                    }

                    SendEntityPhotonPackages(EventCode.BulkPositionUpdate, currentEntities);
                }
            }

            this.ExecutionFiber.Schedule(SendAvatarUpdates, _intervalSendAvatarUpdates);
        }

        private void SendPositions()
        {
            var allEntities = GetAllRelevantEntities();

            var currentEntities = new Dictionary<int, IEntityPhotonPacket>();

            // 104 = BulkPositionUpdate
            foreach (var entity in allEntities.Values)
            {
                currentEntities.Add(entity.Id, CreateEntityPosition(entity));
            }
            SendEntityPhotonPackages(EventCode.BulkPositionUpdate, currentEntities, _previousPositions);

            _previousPositions = currentEntities;

            // Schedule next update
            this.ExecutionFiber.Schedule(SendPositions, _intervalSendPositions);
        }

        private static Dictionary<int, IEntity> GetAllRelevantEntities()
        {
            var currentEntities = new Dictionary<int, IEntity>();

            foreach (var entity in _dawnWorldInstance.Environment.GetCreatures())
            {
                currentEntities.Add(entity.Id, entity);
            }
            foreach (var entity in _dawnWorldInstance.Environment.GetObstacles())
            {
                currentEntities.Add(entity.Id, entity);
            }
            foreach (var entity in _dawnWorldInstance.Environment.GetBullets())
            {
                currentEntities.Add(entity.Id, entity);
            }

            return currentEntities;
        }

        private void SendDawnWorld()
        {
            // Broadcast changes
            {
                // 101 = WorldInformation
                //{
                //    var data = new Dictionary<byte, object>();
                //    data[0] = _dawnWorldInstance.GetWorldInformation();
                //    var eData = new EventData((byte)EventCode.WorldInfo, data);
                //    var sendParameters = new SendParameters { Unreliable = true };
                //    this.PublishEvent(eData, this.Actors, sendParameters);
                //}


                var currentEntities = GetAllRelevantEntities();


                var currentStatuses = new Dictionary<int, IEntityPhotonPacket>();
                // 105 = BulkStatusUpdate
                {
                    foreach (var entity in currentEntities.Values)
                    {
                        currentStatuses.Add(entity.Id, CreateEntityStatus(entity));
                    }

                    SendEntityPhotonPackages(EventCode.BulkStatusUpdate, currentStatuses, _previousStatuses);
                }

                // AddEntity
                {
                    var currentAdded = new Dictionary<int, IEntityPhotonPacket>();

                    foreach (var currentEntityKvp in currentEntities)
                    {
                        if (!_previousStatuses.ContainsKey(currentEntityKvp.Key))
                        {
                            currentAdded.Add(currentEntityKvp.Key, CreateEntityAdded(currentEntityKvp.Value));
                        }
                    }

                    SendEntityPhotonPackages(EventCode.BulkAddEntity, currentAdded, null, false);
                }

                // 103 = destroyed
                {
                    var killedHash = new List<int>();

                    foreach (var previousEntity in _previousStatuses.Keys)
                    {
                        if (!currentEntities.ContainsKey(previousEntity))
                        {
                            killedHash.Add(previousEntity);
                        }
                    }

                    SendKilled(killedHash.ToArray());
               }

                _previousStatuses = currentStatuses;

                // Walls
                //SendWalls();
            }

            this.ExecutionFiber.Schedule(SendDawnWorld, _intervalSendStatus);
        }

        private void SendKilled(int[] killedIds)
        {
            if (killedIds.Length > 0)
            {
                Monitoring.Register_SendKilled();

                // Send killed reliable
                var data = new Dictionary<byte, object>();
                data[0] = killedIds;
                var eData = new EventData((byte)EventCode.Destroyed, data);
                var sendParameters = new SendParameters { Unreliable = false, ChannelId = 1};
                this.PublishEvent(eData, this.Actors, sendParameters);
            }
        }

        private void SendEntityPhotonPackages(EventCode eventCode, Dictionary<int, IEntityPhotonPacket> entityPositions, Dictionary<int, IEntityPhotonPacket> previousPositions = null, bool unreliable = true)
        {
            if (entityPositions.Count == 0)
                return;

            // TODO: warnings when we exceed the max packet-size!
            // => HOW?


            // send a limited amount of packages, starting with the last served
            var sortedEntityPackages = PreprocessEntityPackages(entityPositions, previousPositions);

            // SEND FRAGMENTED
            var sendParameters = unreliable
                                     ? new SendParameters {Unreliable = true}
                                     : new SendParameters {Unreliable = false, ChannelId = 1};

            // Split the list into fragments

            byte index = 0;
            int fragmentIndex = 0;
            var currentDataList = new Dictionary<byte, object>();
            foreach (var package in sortedEntityPackages)
            {
                currentDataList.Add(index++, package.CreatePhotonPacket());

                if (index > _fragmentSize)
                {
                    var eData = new EventData((byte)eventCode, currentDataList);
                    this.PublishEvent(eData, this.Actors, sendParameters);
                    Monitoring.Register_SendEntityPhotonPackages(currentDataList.Count);

                    index = 0;
                    currentDataList.Clear();

                    // TEST, limit #fragment
                    fragmentIndex++;
                    //if (fragmentIndex >= maxFragments)
                    //    break;
                }
            }

            // Send remainder
            if (currentDataList.Count > 0)
            {
                var eData = new EventData((byte)eventCode, currentDataList);
                this.PublishEvent(eData, this.Actors, sendParameters);
                Monitoring.Register_SendEntityPhotonPackages(currentDataList.Count);
            }


            Debug.Assert(fragmentIndex < 50);
        }

        private static IEnumerable<IEntityPhotonPacket> PreprocessEntityPackages(Dictionary<int, IEntityPhotonPacket> entityPositions, Dictionary<int, IEntityPhotonPacket> previousPositions)
        {
            if (previousPositions == null)
            {
                return entityPositions.Values;
            }


            var entityPackages = new List<IEntityPhotonPacket>();

            foreach (var entityStatusKvp in entityPositions)
            {
                // Temp: init to origin to be sure
                entityStatusKvp.Value.LastUpdateSend = new DateTime();

                // Compare the delta for optimizations
                Debug.Assert(previousPositions != null);

                IEntityPhotonPacket previousStatus = null;
                if (previousPositions.TryGetValue(entityStatusKvp.Key, out previousStatus))
                {
                    Debug.Assert(previousStatus != null);
                    if (!entityStatusKvp.Value.HasDeltaChanges(previousStatus))
                        continue;

                    // Copy Timestamp to new Package
                    entityStatusKvp.Value.LastUpdateSend = previousStatus.LastUpdateSend;
                }

                entityPackages.Add(entityStatusKvp.Value);
            }

            var sortedEntityPackages = entityPackages.OrderBy(package => package.LastUpdateSend);
            return sortedEntityPackages;
        }

        private static IEntityPhotonPacket CreateEntityPosition(IEntity entity)
        {
            return new EntityPosition(entity);
        }

        private static IEntityPhotonPacket CreateStaticEntity(IEntity entity)
        {
            return new EntityAdded(entity);
        }

        private static IEntityPhotonPacket CreateEntityStatus(IEntity entity)
        {
            return new EntityStatus(entity, _worldSyncState.IsActive(entity.Id));
        }

        private static IEntityPhotonPacket CreateEntityAdded(IEntity entity)
        {
            return new EntityAdded(entity);
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
                        Monitoring.Register_ReceiveAvatarCommand();
                        HandleAvatorCommand(operationRequest);
                        break;
                    }

                case MyOperationCodes.BulkEntityCommand:
                    {
                        Monitoring.Register_ReceiveBulkEntityCommand();
                        HandleBulkEntityCommand(operationRequest);
                        break;
                    }

                case MyOperationCodes.AddEntity:
                    {
                        Monitoring.Register_ReceiveAddEntity();
                        HandleAddEntity(operationRequest, peer);
                        break;
                    }

                case MyOperationCodes.AddAvatar:
                    {
                        var avatar = _dawnWorldInstance.AddAvatar();

                        // Send response
                        var eData = new Dictionary<byte, object>();
                        eData[0] = avatar.Id;
                        var response = new OperationResponse((byte)MyOperationCodes.AddAvatar, eData);
                        peer.SendOperationResponse(response, new SendParameters { Unreliable = false, ChannelId = 1});

                        break;
                    }

                case MyOperationCodes.LoadWorld:
                    {
                        //var slowObjects = _dawnWorldInstance.Environment.GetObstacles().Where(o => o.Specy == EntityType.Wall).ToList();

                        // Load ALL objects & creatures on LoadWorld => this allows us to use BulkAddEntity
                        var slowObjects = _dawnWorldInstance.Environment.GetObstacles().ToList();
                        var slowCreatures = _dawnWorldInstance.Environment.GetCreatures();
                        slowObjects.AddRange(slowCreatures);

                        var slowObjectsParam = slowObjects.Select(e => CreateStaticEntity(e).CreatePhotonPacket()).ToArray();

                        // Send response
                        var eData = new Dictionary<byte, object>();
                        eData[0] = slowObjectsParam;
                        var response = new OperationResponse((byte)MyOperationCodes.LoadWorld, eData);
                        peer.SendOperationResponse(response, new SendParameters { Unreliable = false, ChannelId = 1 });

                        break;
                    }

                default:
                    // all other operations will be handled by the LiteGame implementation
                    base.ExecuteOperation(peer, operationRequest, sendParameters);
                    break;
            }
        }

        private void HandleAddEntity(OperationRequest operationRequest, LitePeer peer)
        {
            var parameters = (Hashtable)operationRequest.Parameters[0];

            // Add to world
            var entityType = (EntityType)parameters[0];
            var position = new Vector2((float) parameters[1], (float) parameters[2]);
            var angle = (float) parameters[3];
            var spawnPoint = (int) parameters[4];
            var clientId = (int)parameters[5]; // client referenceId of created creature

            var newCreature = _dawnWorldInstance.AddCreature(entityType, position, angle, spawnPoint, clientId);
            Debug.Assert(newCreature != null);

            // Send response
            //var eData = new Dictionary<byte, object>();
            //eData[0] = (newCreature != null) ? newCreature.Id : 0;
            //eData[1] = clientId; // client referenceId of created creature
            //var response = new OperationResponse((byte)MyOperationCodes.AddEntity, eData);
            //peer.SendOperationResponse(response, new SendParameters { Unreliable = false, ChannelId = 1 });
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
            var entityCommands = operationRequest.Parameters.Values.Cast<int[]>();
            foreach (var entityCommand in entityCommands)
            {
                HandleEntityCommand(entityCommand);
            }
        }

        private static void HandleEntityCommand(int[] parameters)
        {
            var entityId = (int)parameters[0];
            var nrOfCommands = parameters.Length-1;

            var commands = new List<AvatarCommand>();
            for (int i=0; i < nrOfCommands; i++)
            {
                commands.Add((AvatarCommand)parameters[1+i]);
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
                // Client not yet in synch with KILL or ADD operation
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
