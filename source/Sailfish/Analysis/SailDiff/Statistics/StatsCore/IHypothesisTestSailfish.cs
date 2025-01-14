using Sailfish.Analysis.SailDiff.Statistics.StatsCore.Distributions;
using System;
using System.Globalization;

namespace Sailfish.Analysis.SailDiff.Statistics.StatsCore;

public interface IHypothesisTestSailfish
{
    DistributionTailSailfish Tail { get; }
    bool Significant { get; }

    double StatisticToPValue(double x);

    double PValueToStatistic(double p);
}

public interface IHypothesisTest
{
    DistributionTailSailfish Tail { get; }

    bool Significant { get; }

    double StatisticToPValue(double x);

    double PValueToStatistic(double p);
}

public interface IHypothesisTestSailfish<out TDist> : IHypothesisTest where TDist : IDistribution
{
    TDist StatisticDistribution { get; }
}

public abstract class HypothesisTest<TDist> : IFormattable, IHypothesisTestSailfish<TDist>, IHypothesisTestSailfish
    where TDist : IUnivariateDistribution
{
    private double alpha = 0.05;

    public double PValue { get; protected set; }

    public double Statistic { get; protected set; }

    public double Size
    {
        get => alpha;
        set
        {
            alpha = value;
            OnSizeChanged();
        }
    }

    public virtual double CriticalValue => PValueToStatistic(alpha);

    public string ToString(string? format, IFormatProvider? formatProvider) => PValue.ToString(format, formatProvider);

    public DistributionTailSailfish Tail { get; protected set; }
    public bool Significant { get; }

    public abstract double StatisticToPValue(double x);

    public abstract double PValueToStatistic(double p);

    public abstract TDist StatisticDistribution { get; set; }

    protected virtual void OnSizeChanged()
    {
    }

    public override string ToString()
    {
        return PValue.ToString(CultureInfo.CurrentCulture);
    }
}