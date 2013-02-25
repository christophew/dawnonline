using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PerformanceMonitoring;

namespace PerformanceMonitorInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            Monitoring.InstallServerCounters();
            Monitoring.InstallClientCounters();
            Monitoring.InstallSimulationCounters();
        }
    }
}
