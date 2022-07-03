﻿using System.Collections.Generic;
using Sailfish.Statistics.StatisticalAnalysis;

namespace Sailfish.Presentation.TTest;

internal interface ITTestComputer
{
    List<NamedTTestResult> ComputeTTest(BeforeAndAfterTrackingFiles beforeAndAfter, TTestSettings settings);
}