﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.Lite;
using SharedConstants;

namespace DawnClient
{
    public class DawnClient : IPhotonPeerListener
    {
        public DawnClientWorld DawnWorld { get; private set; }

        private LitePeer _peer;
        private int _actorId;

        private DateTime _lastUpdateTime = DateTime.Now;
        private HashSet<AvatarCommand> _avatarCommands = new HashSet<AvatarCommand>();
        private Dictionary<int, HashSet<AvatarCommand>> _entityCommands = new Dictionary<int, HashSet<AvatarCommand>>();

        private int _avatarId;
        public int AvatarId { get { return _avatarId; } }

        public class ClientServerIdPair
        {
            public ClientServerIdPair(int serverId, int clientId)
            {
                ServerId = serverId;
                ClientId = clientId;
            }
            public int ServerId;
            public int ClientId;
        }
        private List<ClientServerIdPair> _creatureIds = new List<ClientServerIdPair>();
        public ReadOnlyCollection<ClientServerIdPair> CreatureIds { get { return _creatureIds.AsReadOnly(); } }

        private DawnClientEntity _avatarProxy = new DawnClientEntity();
        public DawnClientEntity Avatar { get { return _avatarProxy; } }

        public bool WorldLoaded { get; private set; }


        #region Events
        public event EventHandler WorldLoadedEvent;

        public class EntityDestroyedEventArgs : EventArgs
        {
            public Hashtable DestroyedIds { get; private set; }

            public EntityDestroyedEventArgs(Hashtable killedIds)
            {
                DestroyedIds = killedIds;
            }
        }
        public delegate void EntityDestroyedEventHandler(object sender, EntityDestroyedEventArgs e);
        public event EntityDestroyedEventHandler EntityDestroyed;
        #endregion

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
            //return _peer.Connect("192.168.1.105:5055", "DawnServer");
        }

        public void Update()
        {
            var now = DateTime.Now;
            long millisecondsSinceLastFrame = (long)(now - _lastUpdateTime).TotalMilliseconds;

            //if (millisecondsSinceLastFrame < 50)
            //    return;

            _lastUpdateTime = now;

            // Send avatar command
            if (_avatarId != 0)
            {
                SendCommands(_avatarId, _avatarCommands);
            }

            // Send creature command
            //foreach (var kvp in _entityCommands)
            //{
            //    SendCommands(kvp.Key, kvp.Value);
            //}

            var bulkCommands = new List<Hashtable>();
            foreach (var kvp in _entityCommands)
            {
                bulkCommands.Add(CreateCommandData(kvp.Key, kvp.Value));
            }
            SendBulkCommands(bulkCommands);
            _entityCommands.Clear();

            _peer.Service();
        }

        private void SendCommands(int id, HashSet<AvatarCommand> commands)
        {
            // Always send commands, even when none selected = used to clear the current action queue on the server
            // => could be optimized: stop doing this when the commands haven't changed a number of times
            //if (commands.Count == 0)
            //    return;

            var eData = new Dictionary<byte, object>();
            var commandsArray = commands.Select(command => (byte)command).ToArray();
            eData[0] = commandsArray;
            eData[1] = id;

            var result = _peer.OpCustom((byte)MyOperationCodes.AvatarCommand, eData, false);
            commands.Clear();
        }

        private void SendBulkCommands(List<Hashtable> positions)
        {
            // TODO: warnings when we exceed the max packet-size!

            // Split the list into fragments
            const int fragmentSize = 50;

            byte index = 0;
            var currentDataList = new Dictionary<byte, object>();
            foreach (var position in positions)
            {
                currentDataList.Add(index++, position);

                if (index > fragmentSize)
                {
                    var result = _peer.OpCustom((byte)MyOperationCodes.BulkEntityCommand, currentDataList, false);

                    index = 0;
                    currentDataList = new Dictionary<byte, object>();
                }
            }

            // Send remainder
            if (currentDataList.Count > 0)
            {
                var result = _peer.OpCustom((byte)MyOperationCodes.BulkEntityCommand, currentDataList, false);
            }
        }

        private Hashtable CreateCommandData(int entityId, HashSet<AvatarCommand> commands)
        {
            var commandData = new Hashtable();
            commandData[0] = entityId;
            commandData[1] = (byte)commands.Count;

            var i = 2;
            foreach (var command in commands)
            {
                commandData[i++] = command;
            }

            return commandData;
        }

        public void RequestCreatureCreationOnServer(EntityType entityType, float x, float y, float angle, int spawnPointId, int clientId)
        {
            Console.WriteLine("RequestCreatureCreationOnServer: " + entityType);

            var creatureData = new Hashtable();
            creatureData.Add(0, entityType);
            creatureData.Add(1, x);
            creatureData.Add(2, y);
            creatureData.Add(3, angle);
            creatureData.Add(4, spawnPointId);
            creatureData.Add(5, clientId);

            var eData = new Dictionary<byte, object>();
            eData[0] = creatureData;

            var result = _peer.OpCustom((byte)MyOperationCodes.AddEntity, eData, true);
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

        public void SendEntityCommand(int entityId, AvatarCommand command)
        {
            if (!_entityCommands.ContainsKey(entityId))
                _entityCommands.Add(entityId, new HashSet<AvatarCommand>());

            _entityCommands[entityId].Add(command);
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

                case (byte)EventCode.WorldInfo:
                    {
                        // World information
                        DawnWorld.WorldInformation = (string) eventData.Parameters[0];

                        break;
                    }
                case (byte)EventCode.BulkPositionUpdate:
                    {
                        // Position update: compressed
                        var entities = eventData.Parameters.Select(kvp => new DawnClientEntity((Hashtable) kvp.Value)).ToList();
                        DawnWorld.UpdateEntities(entities);

                        // Update avatarProxy
                        if (_avatarId != 0)
                        {
                            var avatar = entities.FirstOrDefault(e => e.Id == _avatarId);
                            if (avatar != null)
                            {
                                _avatarProxy.UpdateFrom(avatar);
                            }
                        }

                        break;
                    }

                case (byte)EventCode.Destroyed:
                    // Killed
                    var killedIds = (Hashtable) eventData.Parameters[0];
                    DawnWorld.RemoveEntities((Hashtable)eventData.Parameters[0]);

                    // Fire our event
                    if (this.EntityDestroyed != null)
                    {
                        var eventArgs = new EventArgs();

                        this.EntityDestroyed(this, new EntityDestroyedEventArgs(killedIds));
                    }

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

            switch (operationResponse.OperationCode)
            {
                case (byte)LiteOpCode.Join:
                    {
                        _actorId = (int) operationResponse.Parameters[LiteOpKey.ActorNr];
                        Console.WriteLine(" ->My PlayerNr (or ActorNr) is: " + _actorId);

                        Console.WriteLine("Calling LoadWorld operation");
                        //var opParams = new Dictionary<byte, object>();
                        //opParams[LiteOpKey.Code] = (byte)103;
                        _peer.OpCustom((byte)MyOperationCodes.LoadWorld, null, true);
                        break;
                    }
                // Return from LoadWorld
                case (byte)MyOperationCodes.LoadWorld:
                    {
                        // Get avatorId
                        _avatarId = (int)operationResponse.Parameters[0];
                        Console.WriteLine(" ->My AvatarId is: " + _avatarId);

                        // Get static world objects
                        var entityParam = (Hashtable[])operationResponse.Parameters[1];
                        var staticEntities = entityParam.Select(param => new DawnClientEntity(param)).ToList();
                        Console.WriteLine(" ->Starting entities: " + staticEntities.Count);
                        DawnWorld.UpdateEntities(staticEntities);

                        WorldLoaded = true;

                        // Fire our event
                        if (this.WorldLoadedEvent != null)
                            this.WorldLoadedEvent(this, new EventArgs());

                        break;
                    }
                // Return from AddPredator
                case (byte)MyOperationCodes.AddEntity:
                    {
                        var creatureServerId = (int) operationResponse.Parameters[0];
                        var creatureClientId = (int) operationResponse.Parameters[1];

                        _creatureIds.Add(new ClientServerIdPair(creatureServerId, creatureClientId));
                        break;
                    }

            }
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
    }
}
