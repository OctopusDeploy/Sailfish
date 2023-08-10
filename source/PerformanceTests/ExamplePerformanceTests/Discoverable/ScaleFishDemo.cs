using System;
using System.Threading;
using System.Threading.Tasks;
using Sailfish.Attributes;

namespace PerformanceTests.ExamplePerformanceTests.Discoverable;

[WriteToMarkdown]
[Sailfish(NumIterations = 7, Disabled = false)]
public class ScaleFishDemo
{
    [SailfishVariable(true, 1, 5, 10, 15)]
    public int N { get; set; }

    [SailfishVariable(1, 2)] public int OtherN { get; set; }

    [SailfishMethod]
    public async Task Quadratic(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        await Task.Delay(Convert.ToInt32(N * N) + OtherN, cancellationToken);
    }

    [SailfishMethod]
    public async Task Cubic(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        await Task.Delay(Convert.ToInt32(N * N * N) + OtherN, cancellationToken);
    }

    [SailfishMethod]
    public async Task NLogN(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        await Task.Delay(Convert.ToInt32(N * Math.Log(N)) + OtherN, cancellationToken);
    }
}