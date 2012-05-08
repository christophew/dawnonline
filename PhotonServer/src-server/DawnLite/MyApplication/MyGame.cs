
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DawnGame;
using DawnOnline.Simulation.Entities;
using Lite.Messages;

namespace MyApplication
{
    using Lite;

    using Operations;

    using Photon.SocketServer;

    public class MyGame : LiteGame
    {
        private DawnWorld _dawnWorld = new DawnWorld();
        private DateTime _lastUpdateTime = DateTime.Now;
        private HashSet<int> _previousEntities = new HashSet<int>();

        private void UpdateDawnWorld()
        {
            var now = DateTime.Now;
            long millisecondsSinceLastFrame = (long) (now - _lastUpdateTime).TotalMilliseconds;
            _lastUpdateTime = now;

            //Debug.WriteLine("ms: " + millisecondsSinceLastFrame);

            _dawnWorld.ThinkAll(30, new TimeSpan(millisecondsSinceLastFrame));
            _dawnWorld.ApplyMove(millisecondsSinceLastFrame);
            _dawnWorld.UpdatePhysics(millisecondsSinceLastFrame);
        }

        private void SendDawnWorld()
        {
            // Broadcast changes
            {
                // 101 = WorldInformation
                {
                    var data = new Dictionary<byte, object>();
                    data[0] = _dawnWorld.GetWorldInformation();
                    var eData = new EventData(101, data);
                    var sendParameters = new SendParameters { Unreliable = true };
                    this.PublishEvent(eData, this.Actors, sendParameters);
                }

                var currentEntities = new HashSet<int>();

                // 104 = compressed position update
                {
                    var positionData = new List<Hashtable>();

                    foreach (var entity in _dawnWorld.Environment.GetCreatures())
                    {
                        currentEntities.Add(entity.Id);
                        positionData.Add(CreateEntityData(entity));
                    }
                    foreach (var entity in _dawnWorld.Environment.GetObstacles())
                    {
                        currentEntities.Add(entity.Id);

                        //if (entity.Specy == EntityType.Wall)
                        //    continue;
                        positionData.Add(CreateEntityData(entity));
                    }
                    foreach (var entity in _dawnWorld.Environment.GetBullets())
                    {
                        currentEntities.Add(entity.Id);
                        positionData.Add(CreateEntityData(entity));
                    }

                    SendPositionsEvent(positionData);
                }

                // 103 = destroyed
                {
                    var killedHash = new Hashtable();

                    int index = 0;
                    foreach (var previousEntity in _previousEntities)
                    {
                        if (!currentEntities.Contains(previousEntity))
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
                        var eData = new EventData(103, data);
                        var sendParameters = new SendParameters { Unreliable = false };
                        this.PublishEvent(eData, this.Actors, sendParameters);
                    }
                }

                _previousEntities = currentEntities;

                // Walls
                //SendWalls();
            }

            this.ExecutionFiber.Schedule(SendDawnWorld, 50);
        }

        private void SendPositionsEvent(List<Hashtable> positions)
        {
            // TODO: warnings when we exceed the max packet-size!

            var sendParameters = new SendParameters { Unreliable = true };

            // Split the list into fragments
            const int fragmentSize = 10;

            byte index = 0;
            var currentDataList = new Dictionary<byte, object>();
            foreach (var position in positions)
            {
                currentDataList.Add(index++, position);

                if (index > fragmentSize)
                {
                    var eData = new EventData(104, currentDataList);
                    this.PublishEvent(eData, this.Actors, sendParameters);

                    index = 0;
                    currentDataList = new Dictionary<byte, object>();
                }
            }

            // Send remainder
            if (currentDataList.Count > 0)
            {
                var eData = new EventData(104, currentDataList);
                this.PublishEvent(eData, this.Actors, sendParameters);
            }
        }

        private Hashtable CreateEntityData(IEntity entity)
        {
            var dawnEntity = new Hashtable();
            dawnEntity[0] = entity.Id;
            dawnEntity[1] = entity.Specy;
            dawnEntity[2] = entity.Place.Position.X;
            dawnEntity[3] = entity.Place.Position.Y;
            dawnEntity[4] = entity.Place.Angle;

            var creature = entity as ICreature;
            dawnEntity[5] = creature != null && creature.SpawnPoint != null ? creature.SpawnPoint.Id : 0;
            return dawnEntity;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MyGame"/> class.
        /// </summary>
        /// <param name="gameName">The name of the game.</param>
        public MyGame(string gameName)
            : base(gameName)
        {
            // TODO: do this when a player joined!
            //// Initial update => send static obstacles
            //foreach (var entity in _dawnWorld.Environment.GetObstacles())
            //{
            //    // Ignore walls
            //    if (entity.Specy != EntityType.Wall)
            //        continue;
            //    SendEntityEvent(entity, false);
            //}

            this.ExecutionFiber.Schedule(SendDawnWorld, 1500);
            this.ExecutionFiber.ScheduleOnInterval(UpdateDawnWorld, 1000, 50);
            //this.ExecutionFiber.ScheduleOnInterval(SendWalls, 800, 75);
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

                default:
                    // all other operations will be handled by the LiteGame implementation
                    base.ExecuteOperation(peer, operationRequest, sendParameters);
                    break;
            }
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
