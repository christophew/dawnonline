﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultConfiguration.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Photon.LoadBalancing.LoadBalancer.Configuration
{
    using System.Collections.Generic;

    using Photon.LoadBalancing.LoadShedding;

    internal class DefaultConfiguration
    {
        internal static int[] GetDefaultWeights()
        {
            var loadLevelWeights = new int[(int)FeedbackLevel.Highest + 1]; 

            loadLevelWeights[(int)FeedbackLevel.Lowest] = 40;
            loadLevelWeights[(int)FeedbackLevel.Low] = 30;
            loadLevelWeights[(int)FeedbackLevel.Normal] = 20;
            loadLevelWeights[(int)FeedbackLevel.High] = 10;
            loadLevelWeights[(int)FeedbackLevel.Highest] = 0;

            return loadLevelWeights; 
        }
    }
}
