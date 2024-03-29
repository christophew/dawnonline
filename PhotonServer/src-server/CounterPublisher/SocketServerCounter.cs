﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SocketServerCounter.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   The socket server counter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Photon.CounterPublisher
{
    using ExitGames.Diagnostics.Counter;
    using ExitGames.Diagnostics.Monitoring;

    using Schema = SocketServerCounterSchema;

    /// <summary>
    /// The socket server counter.
    /// </summary>
    [CounterSet(Name = "Photon")]
    public static class SocketServerCounter
    {        
        /// <summary>
        /// The bytes in per second counter.
        /// </summary>
        [PublishCounter("BytesInPerSecond")]
        public static readonly PerformanceCounterReader BytesInPerSecondCounter = 
            CreateCounterReader(Schema.Common.CategoryName, Schema.Common.BytesInPerSecondCounter);
        
        /// <summary>
        /// The bytes out per second counter.
        /// </summary>
        [PublishCounter("BytesOutPerSecond")]
        public static readonly PerformanceCounterReader BytesOutPerSecondCounter = 
            CreateCounterReader(Schema.Common.CategoryName, Schema.Common.BytesOutPerSecondCounter);
        
        /// <summary>
        /// The commands resent per second counter.
        /// </summary>
        [PublishCounter("CommandsResentPerSecond")]
        public static readonly PerformanceCounterReader CommandsResentPerSecondCounter = 
            CreateCounterReader(Schema.Enet.CategoryName, Schema.Enet.CommandsResentPerSecondCounter);

        /// <summary>
        /// The tcp peers counter.
        /// </summary>
        [PublishCounter("TcpPeers")]
        public static readonly PerformanceCounterReader TcpPeersCounter =  
            CreateCounterReader(Schema.Tcp.CategoryName, Schema.Tcp.TcpPeersCounter);
        
        /// <summary>
        /// The timeout disconnect per second counter.
        /// </summary>
        [PublishCounter("TimeoutDisconnectPerSecond")]
        public static readonly PerformanceCounterReader TimeoutDisconnectPerSecondCounter =
            CreateCounterReader(Schema.Enet.CategoryName, Schema.Enet.TimeoutDisconnectPerSecondCounter);

        /// <summary>
        /// The udp peers counter.
        /// </summary>
        [PublishCounter("UdpPeers")]
        public static readonly PerformanceCounterReader UdpPeersCounter =
            CreateCounterReader(Schema.Udp.CategoryName, Schema.Udp.UdpPeersCounter);


        /// <summary>
        ///  The total number of peers (TCP + UDP).
        /// </summary>
        [PublishCounter("Peers")]
        public static readonly PerformanceCounterReader PeersCounter =
            CreateCounterReader(Schema.Common.CategoryName, Schema.Common.PeersTotalCounter);

        /// <summary>
        ///  The total number of active connections.
        /// </summary>
        [PublishCounter("Connections")]
        public static readonly PerformanceCounterReader ConnectionsCounter =
            CreateCounterReader(Schema.Common.CategoryName, Schema.Common.ConnectionsActiveCounter);


        /// <summary>
        ///  The total number of IO Threads.
        /// </summary>
        [PublishCounter("IOThreads")]
        public static readonly PerformanceCounterReader IOThreadsCounter=
            CreateCounterReader(Schema.Threading.CategoryName, Schema.Threading.IoThreadsActiveCounter);
               

        /// <summary>
        ///  The total number of Enet Threads.
        /// </summary>
        [PublishCounter("EnetThreads")]
        public static readonly PerformanceCounterReader EnetThreadsCounter =
            CreateCounterReader(Schema.Threading.CategoryName, Schema.Threading.EnetThreadsActiveCounter);

        /// <summary>
        ///  The total number of Business Logic Threads.
        /// </summary>
        [PublishCounter("BusinessLogicThreads")]
        public static readonly PerformanceCounterReader BusinessLogicThreadsCounter =
            CreateCounterReader(Schema.Threading.CategoryName, Schema.Threading.BusinessLogicThreadsActiveCounter);

        /// <summary>
        ///  The total number of items in the Enet Queue.
        /// </summary>
        [PublishCounter("EnetQueue")]
        public static readonly PerformanceCounterReader EnetQueueCounter =
            CreateCounterReader(Schema.Threading.CategoryName, Schema.Threading.EnetQueueCounter);

        /// <summary>
        ///  The total number of items in the Business Logic Queue.
        /// </summary>
        [PublishCounter("BusinessLogicQueue")]
        public static readonly PerformanceCounterReader BusinessLogicQueueCounter =
            CreateCounterReader(Schema.Threading.CategoryName, Schema.Threading.BusinessLogicQueueCounter);


        /// <summary>
        ///  The number of received commands per second. 
        /// </summary>
        [PublishCounter("MessagesInPerSecond")] public static readonly PerformanceCounterReader
            MessagesInPerSecondCounter =
                CreateCounterReader(Schema.Common.CategoryName, Schema.Common.MessagesInCounterPerSecond);

        /// <summary>
        ///  The number of sent commands per second. 
        /// </summary>
        [PublishCounter("MessagesOutPerSecond")]
        public static readonly PerformanceCounterReader
            MessagesOutPerSecondCounter =
            CreateCounterReader(Schema.Common.CategoryName, Schema.Common.MessagesOutCounterPerSecond);


        [PublishCounter("ReliableCommandsInPerSecond")]
        public static readonly PerformanceCounterReader ReliableCommandsIn = CreateCounterReader(
            Schema.Enet.CategoryName, Schema.Enet.ReliableCommandsInPerSecondCounter);

        [PublishCounter("ReliableCommandsOutPerSecond")]
        public static readonly PerformanceCounterReader ReliableCommandsOut = CreateCounterReader(
            Schema.Enet.CategoryName, Schema.Enet.ReliableCommandsOutPerSecondCounter);
        
        [PublishCounter("UnreliableCommandsInPerSecond")]
        public static readonly PerformanceCounterReader UnreliableCommandsIn = CreateCounterReader(
            Schema.Enet.CategoryName, Schema.Enet.UnreliableCommandsInPerSecondCounter);

        [PublishCounter("UnreliableCommandsOutPerSecond")]
        public static readonly PerformanceCounterReader UnreliableCommandsOut = CreateCounterReader(
            Schema.Enet.CategoryName, Schema.Enet.UnreliableCommandsOutPerSecondCounter); 


        private static PerformanceCounterReader CreateCounterReader(string category, string name)
        {
            return new PerformanceCounterReader(category, name, "_Total");
        }
    }
}