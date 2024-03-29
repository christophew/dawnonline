﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultConfiguration.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Photon.LoadBalancing.LoadShedding.Configuration
{
    using System.Collections.Generic;

    internal class DefaultConfiguration
    {
        internal static List<FeedbackController> GetDefaultControllers()
        {
            var cpuController = new FeedbackController(
            FeedbackName.CpuUsage,
            new Dictionary<FeedbackLevel, int>
                    {
                        { FeedbackLevel.Lowest, 20 },
                        { FeedbackLevel.Low, 35 },
                        { FeedbackLevel.Normal, 50 },
                        { FeedbackLevel.High, 70 },
                        { FeedbackLevel.Highest, 90 }
                    },
            0,
            FeedbackLevel.Lowest);

        var businessLogicQueueController = new FeedbackController(
            FeedbackName.BusinessLogicQueueLength,
            new Dictionary<FeedbackLevel, int>
                    {
                        { FeedbackLevel.Lowest, 10 }, 
                        { FeedbackLevel.Low, 100 }, 
                        { FeedbackLevel.Normal, 200 }, 
                        { FeedbackLevel.High, 400 }, 
                        { FeedbackLevel.Highest, 500 }
                    },
            0,
            FeedbackLevel.Lowest);

        var enetQueueController = new FeedbackController(
            FeedbackName.ENetQueueLength,
            new Dictionary<FeedbackLevel, int>
                    {
                        { FeedbackLevel.Lowest, 10 }, 
                        { FeedbackLevel.Low, 100 }, 
                        { FeedbackLevel.Normal, 200 }, 
                        { FeedbackLevel.High, 400 }, 
                        { FeedbackLevel.Highest, 500 }
                    },
            0,
            FeedbackLevel.Lowest);

        const int MegaByte = 1024 * 1024;
        var thresholdValues = new Dictionary<FeedbackLevel, int> 
                {
                    { FeedbackLevel.Lowest, MegaByte }, 
                    { FeedbackLevel.Normal, 4 * MegaByte }, 
                    { FeedbackLevel.High, 8 * MegaByte }, 
                    { FeedbackLevel.Highest, 10 * MegaByte }
                };
        var bandwidthController = new FeedbackController(FeedbackName.Bandwidth, thresholdValues, 0, FeedbackLevel.Lowest);



        var latencyControllerTcp = new FeedbackController(
            FeedbackName.LatencyTcp,
            new Dictionary<FeedbackLevel, int>
                    {
                        { FeedbackLevel.Lowest, 5 }, 
                        { FeedbackLevel.Low, 20 }, 
                        { FeedbackLevel.Normal, 40 }, 
                        { FeedbackLevel.High, 70 }, 
                        { FeedbackLevel.Highest, 150 }
                    },
            0,
            FeedbackLevel.Lowest);


        var latencyControllerUdp = new FeedbackController(
            FeedbackName.LatencyUdp,
            new Dictionary<FeedbackLevel, int>
                    {
                        { FeedbackLevel.Lowest, 5 }, 
                        { FeedbackLevel.Low, 20 }, 
                        { FeedbackLevel.Normal, 40 }, 
                        { FeedbackLevel.High, 70 }, 
                        { FeedbackLevel.Highest, 150 }
                    },
            0,
            FeedbackLevel.Lowest);
      
            return new List<FeedbackController> { cpuController, bandwidthController, latencyControllerTcp, latencyControllerUdp, businessLogicQueueController, enetQueueController }; 
        }
    }
}
