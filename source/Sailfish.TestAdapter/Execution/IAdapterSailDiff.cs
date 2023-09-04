using System.Threading;
using Sailfish.Analysis.SailDiff;
using Sailfish.Contracts.Public;
using Sailfish.Execution;

namespace Sailfish.TestAdapter.Execution;

internal interface IAdapterSailDiff : ISailDiff
{
    string ComputeTestCaseDiff(
        TestExecutionResult testExecutionResult,
        IExecutionSummary executionSummary,
        SailDiffSettings sailDiffSettings,
        PerformanceRunResult preloadedLastRun,
        CancellationToken cancellationToken);
}