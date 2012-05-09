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

        private LitePeer _peer;
        private DateTime _lastUpdateTime = DateTime.Now;
        private HashSet<AvatarCommand> _avatarCommands = new HashSet<AvatarCommand>();

        public DawnClient()
        {
            _peer = new LitePeer(this, ConnectionProtocol.Udp);
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
            var now = DateTime.Now;
            long millisecondsSinceLastFrame = (long)(now - _lastUpdateTime).TotalMilliseconds;

            if (millisecondsSinceLastFrame < 50)
                return;

            _lastUpdateTime = now;

            // Send avatar command
            if (_avatarCommands.Count > 0)
            {
                var eData = new Dictionary<byte, object>();
                var commands = _avatarCommands.Select(command => (byte)command).ToArray();
                eData[0] = commands;
                // 102 = OperationCode.AvatarCommand
                var result = _peer.OpCustom((byte) 102, eData, false);
                _avatarCommands.Clear();
            }

            _peer.Service();
        }

        public void Disconnect()
        {
            _peer.Disconnect();
        }

        public void SendAvatorCommand(AvatarCommand command)
        {
            _avatarCommands.Add(command);
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
                    {
                        // World information
                        DawnWorld.WorldInformation = (string) eventData.Parameters[0];

                        break;
                    }
                case 104:
                    {
                        // Position update: compressed
                        var entities = eventData.Parameters.Select(kvp => new DawnClientEntity((Hashtable) kvp.Value)).ToList();
                        DawnWorld.UpdateEntities(entities);

                        break;
                    }

                case 103:
                    // Killed
                    DawnWorld.RemoveEntities((Hashtable)eventData.Parameters[0]);

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
