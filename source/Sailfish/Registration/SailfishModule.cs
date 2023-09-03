﻿using Autofac;
using Microsoft.Extensions.Configuration;
using Sailfish.Analysis;
using Sailfish.Analysis.SailDiff;
using Sailfish.Analysis.ScaleFish;
using Sailfish.Contracts.Public;
using Sailfish.Contracts.Serialization.V1;
using Sailfish.Execution;
using Sailfish.Presentation;
using Sailfish.Presentation.Console;
using Sailfish.Presentation.CsvAndJson;
using Sailfish.Presentation.Markdown;
using Sailfish.Statistics;
using Sailfish.Statistics.Tests;
using Sailfish.Statistics.Tests.MWWilcoxonTestSailfish;
using Sailfish.Statistics.Tests.TTestSailfish;
using Sailfish.Statistics.Tests.TwoSampleWilcoxonSignedRankTestSailfish;
using Serilog;

namespace Sailfish.Registration;

public class SailfishModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("sailfish.logging.json", true).Build();

        base.Load(builder);
        builder.Register<ILogger>(
            (c, p) =>
                new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .WriteTo.Console()
                    .MinimumLevel.Verbose()
                    .CreateLogger()).SingleInstance();

        builder.RegisterType<SailfishExecutor>().AsSelf();
        builder.RegisterType<SailFishTestExecutor>().As<ISailFishTestExecutor>();
        builder.RegisterType<TestFilter>().As<ITestFilter>();
        builder.RegisterType<TestListValidator>().As<ITestListValidator>();
        builder.RegisterType<TestCollector>().As<ITestCollector>();
        builder.RegisterType<ParameterCombinator>().As<IParameterCombinator>();
        builder.RegisterType<PropertySetGenerator>().As<IPropertySetGenerator>();
        builder.RegisterType<TestInstanceContainerCreator>().As<ITestInstanceContainerCreator>();
        builder.RegisterType<TestCaseIterator>().As<ITestCaseIterator>();
        builder.RegisterType<StatisticsCompiler>().As<IStatisticsCompiler>();
        builder.RegisterType<ExecutionSummaryCompiler>().As<IExecutionSummaryCompiler>();
        builder.RegisterType<ExecutionSummaryWriter>().As<IExecutionSummaryWriter>();
        builder.RegisterType<FileIo>().As<IFileIo>();
        builder.RegisterType<MarkdownWriter>().As<IMarkdownWriter>();
        builder.RegisterType<ConsoleWriter>().As<IConsoleWriter>();
        builder.RegisterType<PerformanceRunResultFileWriter>().As<IPerformanceRunResultFileWriter>();
        builder.RegisterType<TrackingFileFinder>().As<ITrackingFileFinder>();
        builder.RegisterType<IterationVariableRetriever>().As<IIterationVariableRetriever>();
        builder.RegisterType<TestResultTableContentFormatter>().As<ITestResultTableContentFormatter>();
        builder.RegisterType<SailfishExecutionEngine>().As<ISailfishExecutionEngine>();
        builder.RegisterType<MarkdownTableConverter>().As<IMarkdownTableConverter>().InstancePerDependency();
        builder.RegisterType<TrackingFileParser>().As<ITrackingFileParser>();
        builder.RegisterType<SailDiff>().As<ISailDiff>();
        builder.RegisterType<ScaleFish>().As<IScaleFish>();
        builder.RegisterType<TrackingFileSerialization>().As<ITrackingFileSerialization>();
        
        builder.RegisterType<TTestSailfish>().As<ITTestSailfish>();
        builder.RegisterType<MannWhitneyWilcoxonTestSailfish>().As<IMannWhitneyWilcoxonTestSailfish>();
        builder.RegisterType<TwoSampleWilcoxonSignedRankTestSailfish>().As<ITwoSampleWilcoxonSignedRankTestSailfish>();
        builder.RegisterType<TestPreprocessor>().As<ITestPreprocessor>();
        builder.RegisterType<StatisticalTestExecutor>().As<IStatisticalTestExecutor>();
        builder.RegisterType<PerformanceRunResultAggregator>().As<IPerformanceRunResultAggregator>();

    }
}