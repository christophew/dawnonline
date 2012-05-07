
using System;
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
            long millisecondsSinceLastFrame = (long)(now - _lastUpdateTime).TotalMilliseconds;
            _lastUpdateTime = now;

            //Debug.WriteLine("ms: " + millisecondsSinceLastFrame);

            _dawnWorld.ThinkAll(30, new TimeSpan(millisecondsSinceLastFrame));
            _dawnWorld.ApplyMove(millisecondsSinceLastFrame);
            _dawnWorld.UpdatePhysics(millisecondsSinceLastFrame);


            // Broadcast changes
            {
                // 101 = WorldInformation
                {
                    var data = new Dictionary<byte, object>();
                    data[0] = _dawnWorld.GetWorldInformation();
                    var eData = new EventData(101, data);
                    var sendParameters = new SendParameters {Unreliable = true};
                    this.PublishEvent(eData, this.Actors, sendParameters);
                }

                var currentEntities = new HashSet<int>();

                // 102 = position update
                {
                    foreach (var entity in _dawnWorld.Environment.GetCreatures())
                    {
                        SendEntityEvent(entity);
                        currentEntities.Add(entity.Id);
                    }
                    foreach (var entity in _dawnWorld.Environment.GetObstacles())
                    {
                        // Ignore walls
                        //if (entity.Specy == EntityType.Wall)
                        //    continue;
                        SendEntityEvent(entity);
                        currentEntities.Add(entity.Id);
                    }
                    foreach (var entity in _dawnWorld.Environment.GetBullets())
                    {
                        SendEntityEvent(entity);
                        currentEntities.Add(entity.Id);
                    }
                }

                // 103 = destroyed
                {
                    foreach (var previousEntity in _previousEntities)
                    {
                        if (!currentEntities.Contains(previousEntity))
                        {
                            // Send killed reliable
                            var data = new Dictionary<byte, object>();
                            data[0] = previousEntity;
                            var eData = new EventData(103, data);
                            var sendParameters = new SendParameters { Unreliable = false };
                            this.PublishEvent(eData, this.Actors, sendParameters);
                        }
                    }
                }

                _previousEntities = currentEntities;
            }

            this.ExecutionFiber.Schedule(UpdateDawnWorld, 25);
        }

        // TODO: refactor into seperate data contract
        private void SendEntityEvent(IEntity entity, bool unreliable = true)
        {
            var data = new Dictionary<byte, object>();
            data[0] = entity.Id;
            data[1] = entity.Specy;
            data[2] = entity.Place.Position.X;
            data[3] = entity.Place.Position.Y;
            data[4] = entity.Place.Angle;

            var creature = entity as ICreature;
            data[5] = creature != null && creature.SpawnPoint != null ? creature.SpawnPoint.Id : 0;

            var eData = new EventData(102, data);
            var sendParameters = new SendParameters { Unreliable = unreliable };
            this.PublishEvent(eData, this.Actors, sendParameters);
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

            this.ExecutionFiber.Schedule(UpdateDawnWorld, 1000);
            //this.ExecutionFiber.ScheduleOnInterval(UpdateDawnWorld, 1000, 100);
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
