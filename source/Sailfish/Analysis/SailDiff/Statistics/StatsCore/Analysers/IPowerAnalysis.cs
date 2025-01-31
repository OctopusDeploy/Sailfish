using Sailfish.Analysis.SailDiff.Statistics.StatsCore.Distributions;
using System;

namespace Sailfish.Analysis.SailDiff.Statistics.StatsCore.Analysers;

public interface IPowerAnalysis : ICloneable, IFormattable
{
    DistributionTailSailfish Tail { get; }

    double Power { get; }

    double Size { get; }

    double Samples { get; }

    double Effect { get; }
}