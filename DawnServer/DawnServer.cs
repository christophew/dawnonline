using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DawnGame;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.Lite;

namespace DawnServer
{
    class DawnServer : IPhotonPeerListener
    {
        static void Main(string[] args)
        {
            new DawnServer().Run();
        }

        private PhotonPeer _peer;
        private bool _serverOnline = false;
        private DawnWorld _dawnWorld;

        private DawnServer()
        {
            _peer = new PhotonPeer(this, ConnectionProtocol.Udp);
            _dawnWorld = new DawnWorld();
        }

        void Run()
        {
            //DebugLevel should usally be ERROR or Warning - ALL lets you "see" more details of what the sdk is doing.
            //Output is passed to you in the DebugReturn callback
            _peer.DebugOut = DebugLevel.ALL;
            if (_peer.Connect("127.0.0.1:5055", "Lite"))
            {
                var lastUpdate = DateTime.Now;

                do
                {
                    var now = DateTime.Now;
                    long millisecondsSinceLastUpdate = (long)(now - lastUpdate).TotalMilliseconds;
                    lastUpdate = now;
                    UpdateSimulation(millisecondsSinceLastUpdate);

                    Console.WriteLine("\n Update in: " + millisecondsSinceLastUpdate);

                    //Debug.Write("."); //allows you to "see" the game loop is working, check your output-tab when running from within VS
                    _peer.Service();
                    System.Threading.Thread.Sleep(25);
                }
                while (!Console.KeyAvailable);
            }
            else
            {
                Console.WriteLine("Unknown hostname!");
            }

            Console.WriteLine("Press any key to end program!");
            Console.ReadKey();
            //peer.Disconnect(); //<- uncomment this line to see a faster disconnect/leave on the other clients.
        }

        private bool UpdateSimulation(long millisecondsSinceLastUpdate)
        {
            _dawnWorld.ThinkAll(30, new TimeSpan(millisecondsSinceLastUpdate));
            _dawnWorld.ApplyMove(millisecondsSinceLastUpdate);
            _dawnWorld.UpdatePhysics(millisecondsSinceLastUpdate);

            // Send simulation to Photon

            return true;
        }


        public void DebugReturn(DebugLevel level, string message)
        {
            // level of detail depends on the setting of peer.DebugOut
            Debug.WriteLine("\nDebugReturn:" + message); //check your output-tab when running from within VS
        }

        public void OnOperationResponse(OperationResponse operationResponse)
        {
            if (operationResponse.ReturnCode == 0)
                Console.WriteLine("\n---OnOperationResponse: OK - " + (OpCodeEnum)operationResponse.OperationCode + "(" + operationResponse.OperationCode + ")");
            else
            {
                Console.WriteLine("\n---OnOperationResponse: NOK - " + (OpCodeEnum)operationResponse.OperationCode + "(" + operationResponse.OperationCode + ")\n ->ReturnCode=" + operationResponse.ReturnCode
                  + " DebugMessage=" + operationResponse.DebugMessage);
                return;
            }

            switch (operationResponse.OperationCode)
            {
                case LiteOpCode.Join:

                    // We are connected to Photon
                    _serverOnline = true;

                    int myActorNr = (int)operationResponse.Parameters[LiteOpKey.ActorNr];
                    Console.WriteLine("DawnServer online, actorNr: " + myActorNr);

                    //Console.WriteLine("Calling OpRaiseEvent ...");
                    //var opParams = new Dictionary<byte, object>();
                    //opParams[LiteOpKey.Code] = (byte)101;
                    ////opParams[LiteOpKey.Data] = "Hello World!"; //<- returns an error, server expects a hashtable

                    //Hashtable evData = new Hashtable();
                    //evData[(byte)1] = "Hello Wolrd!";
                    //opParams[LiteOpKey.Data] = evData;
                    //_peer.OpCustom((byte)LiteOpCode.RaiseEvent, opParams, true);
                    break;
            }
        }

        public void OnStatusChanged(StatusCode statusCode)
        {
            Console.WriteLine("\n---OnStatusChanged:" + statusCode);
            switch (statusCode)
            {
                case StatusCode.Connect:
                    Console.WriteLine("DawnServer Calling OpJoin ...");
                    var opParams = new Dictionary<byte, object>();
                    opParams[LiteOpKey.GameId] = "Dawn";
                    _peer.OpCustom(LiteOpCode.Join, opParams, true);
                    break;
                default:
                    break;
            }
        }

        public void OnEvent(EventData eventData)
        {
            Console.WriteLine("\n---OnEvent: " + eventData.Code + "(" + eventData.Code + ")");

            switch (eventData.Code)
            {
                case LiteEventCode.Join:

                    // Something joined: client or server
                    // TODO

                    int actorNrJoined = (int)eventData.Parameters[LiteEventKey.ActorNr];
                    Console.WriteLine(" ->Player" + actorNrJoined + " joined!");

                    int[] actorList = (int[])eventData.Parameters[LiteEventKey.ActorList];
                    Console.Write(" ->Total num players in room:" + actorList.Length + ", Actornr List: ");
                    foreach (int actorNr in actorList)
                    {
                        Console.Write(actorNr + ",");
                    }
                    Console.WriteLine("");
                    break;

                // Receive Input from Avator !!
                //case 101:
                //    int sourceActorNr = (int)eventData.Parameters[LiteEventKey.ActorNr];
                //    Hashtable evData = (Hashtable)eventData.Parameters[LiteEventKey.Data];
                //    Console.WriteLine(" ->Player" + sourceActorNr + " say's: " + evData[(byte)1]);
                //    break;
            }
        }

        enum OpCodeEnum : byte
        {
            Join = 255,
            Leave = 254,
            RaiseEvent = 253,
            SetProperties = 252,
            GetProperties = 251
        }

    }
}
