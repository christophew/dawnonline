using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.Lite;
using PerformanceMonitoring;
using SharedConstants;

namespace DawnClient
{
    public class DawnClient : IPhotonPeerListener
    {
        public DawnClientWorld DawnWorld { get; private set; }

        private LitePeer _peer;
        private int _actorId;
        public int InstanceId { get { return _actorId; } }

        private DateTime _lastUpdateTime = DateTime.Now;
        private HashSet<AvatarCommand> _avatarCommands = new HashSet<AvatarCommand>();
        private Dictionary<int, HashSet<AvatarCommand>> _entityCommands = new Dictionary<int, HashSet<AvatarCommand>>();

        private int _avatarId;
        public int AvatarId { get { return _avatarId; } }

        public int MinTimeBetweenSendCommands = 25;

        //public class ClientServerIdPair
        //{
        //    public ClientServerIdPair(int serverId, int clientId)
        //    {
        //        ServerId = serverId;
        //        ClientId = clientId;
        //    }
        //    public int ServerId;
        //    public int ClientId;
        //}
        //private List<ClientServerIdPair> _creatureIds = new List<ClientServerIdPair>();
        //public ReadOnlyCollection<ClientServerIdPair> CreatedCreatureIds { get { return _creatureIds.AsReadOnly(); } }

        //public void CleanupCreatedCreatureIds(HashSet<int> destroyedServerIds)
        //{
        //    _creatureIds.RemoveAll(pair => destroyedServerIds.Contains(pair.ServerId));
        //}

        private List<int> _creatureIds = new List<int>();
        public List<int> CreatedCreatureIds { get { return _creatureIds; } }

        public void CleanupCreatedCreatureIds(HashSet<int> destroyedServerIds)
        {
            _creatureIds.RemoveAll(destroyedServerIds.Contains);
        }


        private DawnClientEntity _avatarProxy = new DawnClientEntity();
        public DawnClientEntity Avatar { get { return _avatarProxy; } }

        public bool WorldLoaded { get; private set; }


        #region Events
        public event EventHandler WorldLoadedEvent;

        public class EntityDestroyedEventArgs : EventArgs
        {
            public int[] DestroyedIds { get; private set; }

            public EntityDestroyedEventArgs(int[] killedIds)
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
            //return _peer.Connect("127.0.0.1:5055", "DawnServer");
            return _peer.Connect("dawnserver:5055", "DawnServer");
            //return _peer.Connect("192.168.1.105:5055", "DawnServer");
        }

        public void SendCommandsToServer()
        {
            Monitoring.Register_SendCommandsToServer(InstanceId);

            var now = DateTime.Now;
            long millisecondsSinceLastFrame = (long)(now - _lastUpdateTime).TotalMilliseconds;

            if (millisecondsSinceLastFrame < MinTimeBetweenSendCommands)
            {
                _peer.Service();
                return;
            }

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

            var bulkCommands = new List<int[]>();
            foreach (var kvp in _entityCommands)
            {
                bulkCommands.Add(CreateCommandData(kvp.Key, kvp.Value));
            }
            SendBulkCommands(bulkCommands);
            _entityCommands.Clear();

            _peer.Service();
        }

        public void Update()
        {
            Monitoring.Register_Update(InstanceId);
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
            _peer.Service();
            commands.Clear();
        }

        private void SendBulkCommands(List<int[]> positions)
        {
            // TODO: warnings when we exceed the max packet-size!

            // Split the list into fragments
            const int fragmentSize = 40;

            byte index = 0;
            var currentDataList = new Dictionary<byte, object>();
            foreach (var position in positions)
            {
                currentDataList.Add(index++, position);

                if (index > fragmentSize)
                {
                    var result = _peer.OpCustom((byte)MyOperationCodes.BulkEntityCommand, currentDataList, false);
                    _peer.Service();

                    index = 0;
                    currentDataList.Clear();
                }
            }

            // Send remainder
            if (currentDataList.Count > 0)
            {
                var result = _peer.OpCustom((byte)MyOperationCodes.BulkEntityCommand, currentDataList, false);
                _peer.Service();
            }
        }

        private static int[] CreateCommandData(int entityId, HashSet<AvatarCommand> commands)
        {
            var commandData = new int[1 + commands.Count];
            commandData[0] = entityId;

            var i = 1;
            foreach (var command in commands)
            {
                commandData[i++] = (int)command;
            }

            return commandData;
        }

        public void RequestCreatureCreationOnServer(EntityType entityType, float x, float y, float angle, int spawnPointId, int clientId)
        {
            Console.WriteLine("RequestCreatureCreationOnServer: " + entityType);

            // Register in my created creatures
            _creatureIds.Add(clientId);

            var creatureData = new Hashtable();
            creatureData.Add(0, entityType);
            creatureData.Add(1, x);
            creatureData.Add(2, y);
            creatureData.Add(3, angle);
            creatureData.Add(4, spawnPointId);
            creatureData.Add(5, clientId);

            var eData = new Dictionary<byte, object>();
            eData[0] = creatureData;

            var result = _peer.OpCustom((byte)MyOperationCodes.AddEntity, eData, true, 1);
            _peer.Service();

        }

        public void RequestAvatarCreationOnServer()
        {
            Console.WriteLine("RequestAvatarCreationOnServer");

            var eData = new Dictionary<byte, object>();
            var result = _peer.OpCustom((byte)MyOperationCodes.AddAvatar, null, true, 1);
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
                        Monitoring.Register_ReceiveBulkPositionUpdate(InstanceId);

                        // Position update: compressed
                        var entities = eventData.Parameters.Select(kvp => DawnClientEntity.CreatePositionUpdate((Hashtable) kvp.Value)).ToList();
                        DawnWorld.UpdateEntities(entities, false);

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
                case (byte)EventCode.BulkStatusUpdate:
                    {
                        Monitoring.Register_ReceiveBulkStatusUpdate(InstanceId);

                        // Position update: compressed
                        var entities = eventData.Parameters.Select(kvp => DawnClientEntity.CreateStatusUpdate((Hashtable)kvp.Value)).ToList();
                        DawnWorld.UpdateEntities(entities, true);

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
                    {
                        Monitoring.Register_ReceiveDestroyedCounter(InstanceId);

                        // Killed
                        var killedIds = (int[]) eventData.Parameters[0];
                        DawnWorld.RemoveEntities(killedIds);

                        // Fire our event
                        if (this.EntityDestroyed != null)
                        {
                            var eventArgs = new EventArgs();

                            this.EntityDestroyed(this, new EntityDestroyedEventArgs(killedIds));
                        }

                        break;
                    }
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
                        _peer.OpCustom((byte)MyOperationCodes.LoadWorld, null, true, 1);
                        break;
                    }
                // Return from LoadWorld
                case (byte)MyOperationCodes.AddAvatar:
                    {
                        // Get avatorId
                        _avatarId = (int) operationResponse.Parameters[0];
                        Console.WriteLine(" ->My AvatarId is: " + _avatarId);

                        break;
                    }
                    // Return from LoadWorld
                case (byte)MyOperationCodes.LoadWorld:
                    {
                        // Get static world objects
                        var entityParam = (Hashtable[])operationResponse.Parameters[0];
                        var staticEntities = entityParam.Select(param => DawnClientEntity.CreateStaticUpdate(param)).ToList();
                        Console.WriteLine(" ->Starting entities: " + staticEntities.Count);
                        DawnWorld.UpdateEntities(staticEntities, true);

                        WorldLoaded = true;

                        // Fire our event
                        if (this.WorldLoadedEvent != null)
                            this.WorldLoadedEvent(this, new EventArgs());

                        break;
                    }
                // Return from AddPredator
                case (byte)MyOperationCodes.AddEntity:
                    {
                        throw new NotSupportedException("obsolete");
                        //var creatureServerId = (int) operationResponse.Parameters[0];
                        //var creatureClientId = (int) operationResponse.Parameters[1];

                        //_creatureIds.Add(new ClientServerIdPair(creatureServerId, creatureClientId));
                        //break;
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
                    _peer.OpCustom(LiteOpCode.Join, opParams, true, 1);

                    break;
                default:
                    break;
            }
        }
    }
}
