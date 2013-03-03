using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PerformanceMonitoring
{
    public class Monitoring
    {
        private const string _serverCategoryName = "DawnServer";
        private const string _sendEntityPhotonPackagesName = "SendEntityPhotonPackages";
        private const string _sendEntityPhotonPackageFragmentsName = "SendEntityPhotonPackageFragments";

        private const string _receiveAvatarCommandName = "ReceiveAvatarCommand";
        private const string _receiveBulkEntityCommandName = "ReceiveBulkEntityCommand";
        private const string _receiveAddEntityName = "ReceiveAddEntity";

        private static PerformanceCounter _sendPositionsCounter;
        private static PerformanceCounter _sendPositionsFragmentCounter;
        private static PerformanceCounter _receiveAvatarCommandCounter;
        private static PerformanceCounter _receiveBulkEntityCommandCounter;
        private static PerformanceCounter _receiveAddEntityCounter;


        private const string _clientCategoryName = "DawnClient";
        private const string _receiveBulkPositionUpdateName = "ReceiveBulkPositionUpdate";
        private const string _receiveBulkStatusUpdateName = "ReceiveBulkStatusUpdate";
        private const string _receiveDestroyedName = "ReceiveDestroyed";

        private static PerformanceCounter _receiveBulkPositionUpdateCounter;
        private static PerformanceCounter _receiveBulkStatusUpdateCounter;
        private static PerformanceCounter _receiveDestroyedCounter;
        private static PerformanceCounter _sendCommandsToServerCounter;
        private static PerformanceCounter _updateCounter;


        private const string _simulationCategoryName = "DawnSimulation";
        private const string _thinkCounterName = "ThinkCounter";
        private const string _thinkTimeName = "ThinkTime";
        private const string _sendCommandsToServerName = "SendCommandsToServer";
        private const string _updateName = "Update";

        private static PerformanceCounter _thinkCounter;
        private static PerformanceCounter _thinkTime;

        public static void Register_SendEntityPhotonPackages(int amount)
        {
            PrepareCounter(ref _sendPositionsCounter, _serverCategoryName, _sendEntityPhotonPackagesName);
            _sendPositionsCounter.IncrementBy(amount);

            PrepareCounter(ref _sendPositionsFragmentCounter, _serverCategoryName, _sendEntityPhotonPackageFragmentsName);
            _sendPositionsFragmentCounter.Increment();
        }

        public static void Register_ReceiveAvatarCommand()
        {
            PrepareCounter(ref _receiveAvatarCommandCounter, _serverCategoryName, _receiveAvatarCommandName);
            _receiveAvatarCommandCounter.Increment();
        }

        public static void Register_ReceiveBulkEntityCommand()
        {
            PrepareCounter(ref _receiveBulkEntityCommandCounter, _serverCategoryName, _receiveBulkEntityCommandName);
            _receiveBulkEntityCommandCounter.Increment();
        }

        public static void Register_ReceiveAddEntity()
        {
            PrepareCounter(ref _receiveAddEntityCounter, _serverCategoryName, _receiveAddEntityName);
            _receiveAddEntityCounter.Increment();
        }

        public static void Register_ReceiveBulkPositionUpdate(int instanceId)
        {
            Debug.Assert(instanceId != 0);
            PrepareCounter(ref _receiveBulkPositionUpdateCounter, _clientCategoryName, _receiveBulkPositionUpdateName, instanceId);
            _receiveBulkPositionUpdateCounter.Increment();
        }

        public static void Register_ReceiveBulkStatusUpdate(int instanceId)
        {
            Debug.Assert(instanceId != 0);
            PrepareCounter(ref _receiveBulkStatusUpdateCounter, _clientCategoryName, _receiveBulkStatusUpdateName, instanceId);
            _receiveBulkStatusUpdateCounter.Increment();
        }

        public static void Register_ReceiveDestroyedCounter(int instanceId)
        {
            Debug.Assert(instanceId != 0);
            PrepareCounter(ref _receiveDestroyedCounter, _clientCategoryName, _receiveDestroyedName, instanceId);
            _receiveDestroyedCounter.Increment();
        }

        public static void Register_Think(int instanceId, int timeInMs, int nrProcesses)
        {
            Debug.Assert(instanceId != 0);
            PrepareCounter(ref _thinkCounter, _simulationCategoryName, _thinkCounterName, instanceId);
            _thinkCounter.IncrementBy(nrProcesses);
            PrepareCounter(ref _thinkTime, _simulationCategoryName, _thinkTimeName, instanceId);
            _thinkTime.IncrementBy(timeInMs);
        }

        public static void Register_SendCommandsToServer(int instanceId)
        {
            Debug.Assert(instanceId != 0);
            PrepareCounter(ref _sendCommandsToServerCounter, _clientCategoryName, _sendCommandsToServerName, instanceId);
            _sendCommandsToServerCounter.Increment();
        }

        public static void Register_Update(int instanceId)
        {
            Debug.Assert(instanceId != 0);
            PrepareCounter(ref _updateCounter, _clientCategoryName, _updateName, instanceId);
            _updateCounter.Increment();
        }

        private static void PrepareCounter(ref PerformanceCounter counter, string category, string name)
        {
            if (counter == null)
            {
                counter = new PerformanceCounter(category, name, false);
                counter.RawValue = 0;
            }
        }

        private static void PrepareCounter(ref PerformanceCounter counter, string category, string name, int instanceId)
        {
            if (counter == null)
            {
                counter = new PerformanceCounter(category, name, instanceId.ToString() , false);
                counter.RawValue = 0;
            }
        }

        public static void InstallServerCounters()
        {
            // Temp: always delete
            if (PerformanceCounterCategory.Exists(_serverCategoryName))
                PerformanceCounterCategory.Delete(_serverCategoryName);

            if (!PerformanceCounterCategory.Exists(_serverCategoryName))
            {
                var counters = new CounterCreationDataCollection();

                //// 1. counter for counting totals: PerformanceCounterType.NumberOfItems32
                //{
                //    var totalOps = new CounterCreationData();
                //    totalOps.CounterName = "NumberOfItems32";
                //    totalOps.CounterHelp = "hello counters1";
                //    totalOps.CounterType = PerformanceCounterType.NumberOfItems32;
                //    counters.Add(totalOps);
                //}


                counters.Add(CreateCounter(_sendEntityPhotonPackagesName));
                counters.Add(CreateCounter(_sendEntityPhotonPackageFragmentsName));

                counters.Add(CreateCounter(_receiveAvatarCommandName));
                counters.Add(CreateCounter(_receiveBulkEntityCommandName));
                counters.Add(CreateCounter(_receiveAddEntityName));

                //{
                //    var totalOps = new CounterCreationData();
                //    totalOps.CounterName = "AverageTimer32";
                //    totalOps.CounterHelp = "hello counters3";
                //    totalOps.CounterType = PerformanceCounterType.AverageTimer32;
                //    counters.Add(totalOps);
                //}

                //{
                //    var totalOps = new CounterCreationData();
                //    totalOps.CounterName = "AverageBase";
                //    totalOps.CounterHelp = "hello counters4";
                //    totalOps.CounterType = PerformanceCounterType.AverageBase;
                //    counters.Add(totalOps);
                //}

                // create new category with the counters above
                PerformanceCounterCategory.Create(_serverCategoryName, "todo: help", PerformanceCounterCategoryType.SingleInstance , counters);
            }
        }

        public static void InstallClientCounters()
        {
            // Temp: always delete
            if (PerformanceCounterCategory.Exists(_clientCategoryName))
                PerformanceCounterCategory.Delete(_clientCategoryName);

            if (!PerformanceCounterCategory.Exists(_clientCategoryName))
            {
                var counters = new CounterCreationDataCollection();

                counters.Add(CreateCounter(_receiveBulkPositionUpdateName));
                counters.Add(CreateCounter(_receiveBulkStatusUpdateName));
                counters.Add(CreateCounter(_receiveDestroyedName));
                counters.Add(CreateCounter(_sendCommandsToServerName));
                counters.Add(CreateCounter(_updateName));

                // create new category with the counters above
                PerformanceCounterCategory.Create(_clientCategoryName, "todo: help", PerformanceCounterCategoryType.MultiInstance , counters);
            }
        }

        public static void InstallSimulationCounters()
        {
            // Temp: always delete
            if (PerformanceCounterCategory.Exists(_simulationCategoryName))
                PerformanceCounterCategory.Delete(_simulationCategoryName);

            if (!PerformanceCounterCategory.Exists(_simulationCategoryName))
            {
                var counters = new CounterCreationDataCollection();

                counters.Add(CreateCounter(_thinkCounterName));
                counters.Add(CreateCounter(_thinkTimeName));

                // create new category with the counters above
                PerformanceCounterCategory.Create(_simulationCategoryName, "todo: help", PerformanceCounterCategoryType.MultiInstance, counters);
            }
        }

        private static CounterCreationData CreateCounter(string name)
        {
            var totalOps = new CounterCreationData();
            totalOps.CounterName = name;
            //totalOps.CounterHelp = "";
            totalOps.CounterType = PerformanceCounterType.RateOfCountsPerSecond32;
            return totalOps;
        }
    }
}
