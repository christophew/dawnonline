using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.Lite;
using PerformanceMonitoring;

namespace TestClient
{
    class TestPeerListener : IPhotonPeerListener
    {
        private LitePeer _peer;
        private int _actorId;
        public int InstanceId { get { return _actorId; } }


        public TestPeerListener()
        {
            //_peer = new LitePeer(this, ConnectionProtocol.Udp);
            _peer = new LitePeer(this, ConnectionProtocol.Udp);

            // Testing
            //_peer.TimePingInterval = 100;
            //_peer.DisconnectTimeout = 10000;
            _peer.SentCountAllowance = 5;
        }

        public bool Connect()
        {
            //DebugLevel should usally be ERROR or Warning - ALL lets you "see" more details of what the sdk is doing.
            //Output is passed to you in the DebugReturn callback
            _peer.DebugOut = DebugLevel.ALL;

            //return _peer.Connect("127.0.0.1:5055", "DawnServer");
            return _peer.Connect("dawnserver:5055", "TestPhotonApp");
            //return _peer.Connect("192.168.1.105:5055", "DawnServer");
        }


        public void Update()
        {
            _peer.Service();
        }



        public void Disconnect()
        {
            _peer.Disconnect();
        }

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
            }
        }

        public void OnOperationResponse(OperationResponse operationResponse)
        {
            if (operationResponse.ReturnCode != 0)
            {
                Console.WriteLine("\n---OnOperationResponse: NOK - " + operationResponse.OperationCode + "(" + operationResponse.OperationCode + ")\n ->ReturnCode=" + operationResponse.ReturnCode + " DebugMessage=" + operationResponse.DebugMessage);
                return;
            }

            Console.WriteLine("\n---OnOperationResponse: OK - " + operationResponse.OperationCode);

            switch (operationResponse.OperationCode)
            {
                case (byte)LiteOpCode.Join:
                    {
                        _actorId = (int) operationResponse.Parameters[LiteOpKey.ActorNr];
                        Console.WriteLine(" ->My PlayerNr (or ActorNr) is: " + _actorId);

                        //Console.WriteLine("Calling LoadWorld operation");
                        //_peer.OpCustom((byte)MyOperationCodes.LoadWorld, null, true, 1);
                        break;
                    }
            }
        }

        public void SendTestEvent()
        {
            Console.WriteLine("SendTestEvent");
            //_peer.OpCustom((byte)200, null, false, 1);
            _peer.OpRaiseEvent(200, null, false, 1);
            Monitoring.Register_SendTest(InstanceId);
        }

        public void OnStatusChanged(StatusCode statusCode)
        {
            Console.WriteLine("\n---OnStatusChanged:" + statusCode);
            Console.WriteLine(_peer.VitalStatsToString(true));

            Console.WriteLine("TrafficStatsGameLevel:");
            Console.WriteLine(_peer.TrafficStatsGameLevel.ToString());

            switch (statusCode)
            {
                case StatusCode.Connect:
                    Console.WriteLine("DawnClient Calling OpJoin ...");
                    var opParams = new Dictionary<byte, object>();
                    opParams[LiteOpKey.GameId] = "Dawn";
                    _peer.OpCustom(LiteOpCode.Join, opParams, true, 1);

                    break;
                case StatusCode.Disconnect:
                    Console.WriteLine("Disconnect");
                    Debug.Assert(false, "Disconnect");
                    break;
                case StatusCode.DisconnectByServer:
                    Console.WriteLine("DisconnectByServer");
                    Debug.Assert(false, "DisconnectByServer");
                   break;
                case StatusCode.DisconnectByServerLogic:
                   Console.WriteLine("DisconnectByServerLogic");
                   Debug.Assert(false, "DisconnectByServerLogic");
                   break;
                default:
                    break;
            }
        }

        public void LogDebugInfo()
        {
            _peer.TrafficStatsEnabled = true;
            //_peer.DebugOut = DebugLevel.ALL; // set in .Connect

            //Console.WriteLine("QueuedIncomingCommands: " + _peer.QueuedIncomingCommands);
            //Console.WriteLine("QueuedOutgoingCommands: " + _peer.QueuedOutgoingCommands);
            Console.WriteLine("- RoundTripTimeVariance: " + _peer.RoundTripTimeVariance);
            Console.WriteLine("- RoundTripTime: " + _peer.RoundTripTime);

            //Console.WriteLine("- RoundTripTime: " + _peer.);

            //Console.WriteLine("LongestDeltaBetweenDispatching: " + _peer.TrafficStatsGameLevel.LongestDeltaBetweenDispatching);
            //Console.WriteLine("LongestOpResponseCallback: " + _peer.TrafficStatsGameLevel.LongestOpResponseCallback);

            //Console.WriteLine("ToStringVitalStats: " + _peer.TrafficStatsGameLevel.ToStringVitalStats());

            //Console.WriteLine("ByteCountLastOperation: " + _peer.ByteCountLastOperation);
            //Console.WriteLine("ByteCountCurrentDispatch: " + _peer.ByteCountCurrentDispatch);

            //Console.WriteLine("ByteCountCurrentDispatch: " + _peer.TrafficStatsIncoming.);

        }
    }
}
