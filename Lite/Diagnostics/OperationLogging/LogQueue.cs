﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogQueue.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Lite.Diagnostics.OperationLogging
{
    using System.Collections.Generic;
    using System.Text;

    using ExitGames.Logging;

    public class LogQueue
    {
        public const int DefaultCapacity = 1000;

        #region Constants and Fields

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private readonly int capacity;

        private readonly string name;

        private readonly Queue<LogEntry> queue;

        #endregion

        #region Constructors and Destructors

        public LogQueue(string name, int capacity)
        {
            this.capacity = capacity;
            this.queue = new Queue<LogEntry>(capacity);
            this.name = name; 
        }

        #endregion

        #region Public Methods

        public void Add(LogEntry value)
        {
            if (log.IsDebugEnabled)
            {
                if (this.queue.Count == this.capacity)
                {
                    this.queue.Dequeue();
                }

                this.queue.Enqueue(value);
            }
        }

        public void WriteLog()
        {
            if (log.IsDebugEnabled)
            {
                LogEntry[] logEntries = this.queue.ToArray();
                var sb = new StringBuilder(logEntries.Length + 1); 
                sb.AppendFormat("OperationLog for Game {0}:", this.name).AppendLine();
                foreach (LogEntry entry in logEntries)
                {
                    sb.AppendFormat("{0}: {1}", this.name, entry).AppendLine();
                }
                
                log.Debug(sb.ToString());
            }
        }

        #endregion
    }
}