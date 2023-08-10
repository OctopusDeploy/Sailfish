using System;
using Sailfish.Analysis.Scalefish.CurveFitting;

namespace Sailfish.Analysis.Scalefish.ComplexityFunctions;

public class LogLinear : ComplexityFunction
{
    public LogLinear(IFitnessCalculator fitnessCalculator) : base(fitnessCalculator)
    {
    }

    public override double Compute(double n, double scale, double bias)
    {
        return scale * (n * Math.Log(n, 2)) + bias;
    }

    public override string Name { get; set; } = nameof(LogLinear);
    public override string OName { get; set; } = "O(nlog_2(n)";
    public override string Quality { get; set; } = "Okay";
}