
using System;
using System.Collections.Generic;
using DawnGame;
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
        private MyPeer _peer;

        private void UpdateDawnWorld()
        {
            var now = DateTime.Now;
            long millisecondsSinceLastFrame = (long)(now - _lastUpdateTime).TotalMilliseconds;
            _lastUpdateTime = now;

            _dawnWorld.ThinkAll(30, new TimeSpan(millisecondsSinceLastFrame));
            _dawnWorld.ApplyMove(millisecondsSinceLastFrame);
            _dawnWorld.UpdatePhysics(millisecondsSinceLastFrame);


            // Broadcast changes
            //this.HandleRaiseEventOperation(this.);
            var data = new Dictionary<byte, object>();
            data[(byte)1] = _dawnWorld.GetWorldInformation();
            EventData eData = new EventData(101, data);
            SendParameters sendParameters = new SendParameters();
            this.PublishEvent(eData, this.Actors, sendParameters);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MyGame"/> class.
        /// </summary>
        /// <param name="gameName">The name of the game.</param>
        public MyGame(string gameName)
            : base(gameName)
        {
            //this.ScheduleDawnWorldUpdate();
            this.ExecutionFiber.ScheduleOnInterval(UpdateDawnWorld, 1000, 50);

            System.Diagnostics.Debugger.Launch(); 
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
