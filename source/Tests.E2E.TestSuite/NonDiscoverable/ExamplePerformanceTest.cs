using Demo.API;
using Microsoft.AspNetCore.Mvc.Testing;
using Sailfish.Attributes;
using Sailfish.Logging;
using Tests.E2E.TestSuite.Utils;

// Tests here are automatically discovered and executed
namespace Tests.E2E.TestSuite.NonDiscoverable;

[WriteToMarkdown]
[WriteToCsv]
[Sailfish(1, 0, Disabled = true)]
public class ExamplePerformanceTest : TestBase
{
    private readonly ILogger logger;

    public ExamplePerformanceTest(WebApplicationFactory<DemoApp> factory, ILogger logger) : base(factory)
    {
        this.logger = logger;
    }

    [SailfishVariable(200, 300)] public int WaitPeriod { get; set; }
    [SailfishVariable(1, 2)] public int NTries { get; set; } // try to avoid multiple variables if you can manage

    [SailfishGlobalSetup]
    public void GlobalSetup(CancellationToken cancellationToken)
    {
        // logger.Information("This is the Global Setup");
    }

    [SailfishMethodSetup]
    public void ExecutionMethodSetup(CancellationToken cancellationToken)
    {
        // logger.Information("This is the Execution Method Setup");
    }

    [SailfishIterationSetup]
    public void IterationSetup(CancellationToken cancellationToken)
    {
        // logger.Warning("This is the Iteration Setup - use sparingly");
    }

    [SailfishMethod]
    public async Task WaitPeriodPerfTest(CancellationToken cancellationToken)
    {
        await Task.Delay(WaitPeriod, cancellationToken);
        await Client.GetStringAsync("/", cancellationToken);
    }

    [SailfishMethod]
    public async Task Other(CancellationToken cancellationToken)
    {
        await Task.Delay(WaitPeriod, cancellationToken);
        await Task.CompletedTask;
    }

    [SailfishIterationTeardown]
    public void IterationTeardown(CancellationToken cancellationToken)
    {
        // logger.Warning("This is the Iteration Teardown - use sparingly");
    }

    [SailfishMethodTeardown]
    public void ExecutionMethodTeardown(CancellationToken cancellationToken)
    {
        // logger.Verbose("This is the Execution Method Teardown");
    }

    [SailfishGlobalTeardown]
    public override async Task GlobalTeardown(CancellationToken cancellationToken)
    {
        await Task.Yield();
        // logger.Verbose("This is the Global Teardown");
    }
}