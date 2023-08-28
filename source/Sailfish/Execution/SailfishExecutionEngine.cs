using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using Sailfish.Attributes;
using Sailfish.Exceptions;
using Sailfish.Program;
using Sailfish.Utils;
using Serilog;

namespace Sailfish.Execution;

internal class SailfishExecutionEngine : ISailfishExecutionEngine
{
    private readonly ITestCaseIterator testCaseIterator;
    private readonly IRunSettings runSettings;

    public SailfishExecutionEngine(ITestCaseIterator testCaseIterator, IRunSettings runSettings)
    {
        this.testCaseIterator = testCaseIterator;
        this.runSettings = runSettings;
    }

    /// <summary>
    /// This method is the main entry point for execution in both the main library, as well as the test adapter
    /// A test instance container is basically a single SailfishMethod from a Sailfish test type. The property
    /// matrix is provided to each test instance container, and each container will produce as many instances
    /// as are specified by the property tensor
    /// </summary>
    /// <param name="testProviderIndex"></param>
    /// <param name="totalTestProviderCount"></param>
    /// <param name="testProvider"></param>
    /// <param name="memoryCache"></param>
    /// <param name="providerPropertiesCacheKey"></param>
    /// <param name="preCallback"></param>
    /// <param name="callback"></param>
    /// <param name="exceptionCallback"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<TestExecutionResult>> ActivateContainer(
        [Range(1, int.MaxValue)] int testProviderIndex,
        [Range(1, int.MaxValue)] int totalTestProviderCount,
        TestInstanceContainerProvider testProvider,
        MemoryCache memoryCache,
        string providerPropertiesCacheKey,
        Action<TestInstanceContainer>? preCallback = null,
        Action<TestExecutionResult, TestInstanceContainer>? callback = null,
        Action<TestInstanceContainer?>? exceptionCallback = null,
        Action<TestInstanceContainer?>? testDisabledCallback = null,
        CancellationToken cancellationToken = default)
    {
        if (testProviderIndex > totalTestProviderCount)
        {
            throw new SailfishException($"The test provider index {testProviderIndex} cannot be greater than total test provider count {totalTestProviderCount}");
        }

        var currentPropertyTensorIndex = 0;
        var totalPropertyTensorElements = Math.Max(testProvider.GetNumberOfPropertySetsInTheQueue() - 1, 0);
        var instanceContainerEnumerator = testProvider.ProvideNextTestInstanceContainer().GetEnumerator();

        try
        {
            instanceContainerEnumerator.MoveNext();
        }
        catch (Exception ex)
        {
            exceptionCallback?.Invoke(instanceContainerEnumerator.Current);

            await DisposeOfTestInstance(instanceContainerEnumerator.Current);
            instanceContainerEnumerator.Dispose();
            var msg = $"Error resolving test from {testProvider.Test.FullName}";
            Log.Logger.Fatal(ex, "{Message}", msg);
            if (exceptionCallback is null) throw;
            var exceptionResult = new TestExecutionResult(testProvider, ex);
            return new List<TestExecutionResult>() { exceptionResult };
        }

        var results = new List<TestExecutionResult>();

        if (testProvider.Test.GetCustomAttributes<SailfishAttribute>().Single().Disabled)
        {
            testDisabledCallback?.Invoke(null);
            instanceContainerEnumerator.Dispose();
            return results;
        }

        do
        {
            var testMethodContainer = instanceContainerEnumerator.Current;
            try
            {
                if (memoryCache.Contains(providerPropertiesCacheKey))
                {
                    var savedState = (PropertiesAndFields)memoryCache.Get(providerPropertiesCacheKey);
                    savedState.ApplyPropertiesAndFieldsTo(testMethodContainer.Instance);
                }

                preCallback?.Invoke(testMethodContainer);
                TestCaseCountPrinter.PrintCaseUpdate(testMethodContainer.TestCaseId.DisplayName);

                if (ShouldCallGlobalSetup(testProviderIndex, currentPropertyTensorIndex))
                {
                    try
                    {
                        await testMethodContainer.CoreInvoker.GlobalSetup(cancellationToken);
                        memoryCache.Add(new CacheItem(providerPropertiesCacheKey, testMethodContainer.Instance.RetrievePropertiesAndFields()), new CacheItemPolicy());
                    }
                    catch (Exception ex)
                    {
                        return CatchAndReturn(testProvider, ex);
                    }
                }

                try
                {
                    await testMethodContainer.CoreInvoker.MethodSetup(cancellationToken);
                }
                catch (Exception ex)
                {
                    return CatchAndReturn(testProvider, ex);
                }

                if (testMethodContainer.Disabled)
                {
                    testDisabledCallback?.Invoke(testMethodContainer);
                    currentPropertyTensorIndex += 1;
                    await TryMoveNextOrThrow(exceptionCallback, instanceContainerEnumerator);
                    continue;
                }

                var executionResult = await IterateOverVariableCombos(testMethodContainer, cancellationToken);

                if (!executionResult.IsSuccess)
                {
                    return new() { executionResult };
                }

                try
                {
                    await testMethodContainer.CoreInvoker.MethodTearDown(cancellationToken);
                }
                catch (Exception ex)
                {
                    return CatchAndReturn(testProvider, ex);
                }

                if (ShouldCallGlobalTeardown(testProviderIndex, totalTestProviderCount, currentPropertyTensorIndex, totalPropertyTensorElements))
                {
                    try
                    {
                        await testMethodContainer.CoreInvoker.GlobalTeardown(cancellationToken);
                        memoryCache.Remove(providerPropertiesCacheKey);
                    }
                    catch (Exception ex)
                    {
                        return CatchAndReturn(testProvider, ex);
                    }
                }

                callback?.Invoke(executionResult, testMethodContainer);
                results.Add(executionResult);

                if (ShouldDisposeOfInstance(currentPropertyTensorIndex, totalPropertyTensorElements))
                {
                    await DisposeOfTestInstance(testMethodContainer);
                }

                currentPropertyTensorIndex += 1;
            }
            catch (Exception ex)
            {
                var errorResult = new TestExecutionResult(testMethodContainer, ex);
                results.Add(errorResult);
            }
        } while (await TryMoveNextOrThrow(exceptionCallback, instanceContainerEnumerator));

        instanceContainerEnumerator.Dispose();

        return results;
    }

    // private static void AssertTestIsNotDisabled(TestInstanceContainer testInstanceContainer)
    // {
    //     var methodIsDisabled = testInstanceContainer.ExecutionMethod.GetCustomAttributes<SailfishMethodAttribute>().Single().Disabled;
    //     var typeIsDisabled = testInstanceContainer.Type.GetCustomAttributes<SailfishAttribute>().Single().Disabled;
    //     if (methodIsDisabled || typeIsDisabled) throw new TestDisabledException();
    // }

    private static async Task<bool> TryMoveNextOrThrow(Action<TestInstanceContainer?>? exceptionCallback, IEnumerator<TestInstanceContainer> instanceContainerEnumerator)
    {
        bool continueIterating;
        try
        {
            continueIterating = instanceContainerEnumerator.MoveNext();
        }
        catch (Exception ex)
        {
            if (exceptionCallback is not null)
            {
                exceptionCallback.Invoke(instanceContainerEnumerator.Current);
                await DisposeOfTestInstance(instanceContainerEnumerator.Current);
                continueIterating = true;
            }

            else
            {
                await DisposeOfTestInstance(instanceContainerEnumerator.Current);
                throw new SailfishIterationException(ex.Message);
            }
        }

        return continueIterating;
    }

    private static List<TestExecutionResult> CatchAndReturn(TestInstanceContainerProvider testProvider, Exception ex)
    {
        var exceptionResult = new TestExecutionResult(testProvider, ex);
        return new List<TestExecutionResult>() { exceptionResult };
    }

    private async Task<TestExecutionResult> IterateOverVariableCombos(TestInstanceContainer testInstanceContainer, CancellationToken cancellationToken = default)
    {
        try
        {
            return await testCaseIterator.Iterate(testInstanceContainer, runSettings.DisableOverheadEstimation, cancellationToken);
        }
        catch (Exception exception)
        {
            if (exception is TestDisabledException) throw;
            return new TestExecutionResult(testInstanceContainer, exception.InnerException ?? exception);
        }
    }

    private static bool ShouldCallGlobalTeardown(int methodIndex, int totalMethodCount, int currentVariableSetIndex, int totalNumVariableSets)
    {
        return methodIndex == totalMethodCount && currentVariableSetIndex == totalNumVariableSets;
    }

    private static bool ShouldDisposeOfInstance(int currentVariableSetIndex, int totalNumVariableSets)
    {
        return currentVariableSetIndex == totalNumVariableSets;
    }

    private static bool ShouldCallGlobalSetup(int testProviderIndex, int currentTestProviderIndex)
    {
        return testProviderIndex == 0 && currentTestProviderIndex == 0;
    }

    private static async Task DisposeOfTestInstance(TestInstanceContainer? instanceContainer)
    {
        switch (instanceContainer?.Instance)
        {
            case IAsyncDisposable asyncDisposable:
                await asyncDisposable.DisposeAsync();
                break;
            case IDisposable disposable:
                disposable.Dispose();
                break;
            default:
            {
                if (instanceContainer is not null)
                {
                    instanceContainer.Instance = null!;
                }

                break;
            }
        }
    }
}