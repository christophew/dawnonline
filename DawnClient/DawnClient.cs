using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.Lite;

namespace DawnClient
{
    public class DawnClient : IPhotonPeerListener
    {
        public DawnClientWorld DawnWorld { get; private set; }

        private PhotonPeer _peer;

        public DawnClient()
        {
            _peer = new PhotonPeer(this, ConnectionProtocol.Udp);
            DawnWorld = new DawnClientWorld();
        }

        public bool Connect()
        {
            //DebugLevel should usally be ERROR or Warning - ALL lets you "see" more details of what the sdk is doing.
            //Output is passed to you in the DebugReturn callback
            _peer.DebugOut = DebugLevel.ALL;
            return _peer.Connect("127.0.0.1:5055", "DawnServer");
        }

        public void Update()
        {
            _peer.Service();
        }

        public void Disconnect()
        {
            _peer.Disconnect();
        }

        #region IPhotonPeerListener Members

        public void DebugReturn(DebugLevel level, string message)
        {
            //throw new NotImplementedException();
        }

        public void OnEvent(EventData eventData)
        {
            //Console.WriteLine("\n---OnEvent: " + eventData.Code + "(" + eventData.Code + ")");

            switch (eventData.Code)
            {
                case LiteEventCode.Join:

                    // Something joined: client or server
                    // TODO
                    break;

                case 101:
                    //int sourceActorNr = (int)eventData.Parameters[LiteEventKey.ActorNr];
                    //Hashtable evData = (Hashtable)eventData.Parameters[LiteEventKey.Data];
                    //var data = evData[(byte) 1];

                    DawnWorld.WorldInformation = (string)eventData.Parameters[0];

                    break;
                case 102:
                    // Position update
                    var entity = new DawnClientEntity(eventData.Parameters);
                    DawnWorld.UpdateEntity(entity);

                    break;

                case 103:
                    // Killed
                    DawnWorld.RemoveEntity((int)eventData.Parameters[0]);

                    break;
            }
        }

        public void OnOperationResponse(OperationResponse operationResponse)
        {
            //throw new NotImplementedException();
        }

        public void OnStatusChanged(StatusCode statusCode)
        {
            //Console.WriteLine("\n---OnStatusChanged:" + statusCode);
            switch (statusCode)
            {
                case StatusCode.Connect:
                    Console.WriteLine("DawnClient Calling OpJoin ...");
                    var opParams = new Dictionary<byte, object>();
                    opParams[LiteOpKey.GameId] = "Dawn";
                    _peer.OpCustom(LiteOpCode.Join, opParams, true);
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}
