﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using MediatR;
using Microsoft.Extensions.Configuration;
using PerformanceTestingConsoleApp.CustomHandlerOverrideExamples;
using PerformanceTests;
using Sailfish.Contracts.Public.Commands;
using Sailfish.Presentation;
using Sailfish.Program;
using Serilog;

// ReSharper disable ClassNeverInstantiated.Global

namespace PerformanceTestingConsoleApp;

public class Program : SailfishProgramBase
{
    public static async Task Main(string[] userRequestedTestNames)
    {
        // your main can call the sailfish main.
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.development.json", true)
            .Build();
        Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();

        await SailfishMain<Program>(userRequestedTestNames);

        // You can access your run result using this global static property if you'd like
        if (RunResult.IsValid)
        {
            var executionSummaries = RunResult.ExecutionSummaries;
            var markdown = new MarkdownTableConverter().ConvertToMarkdownTableString(executionSummaries);
        }
    }

    protected override IEnumerable<Type> SourceTypesProvider()
    {
        // Types used to resolve tests and dependencies
        return new[] { typeof(PerformanceTestProjectDiscoveryAnchor) };
    }

    protected override IEnumerable<Type> RegistrationProviderTypesProvider()
    {
        // Types used to resolve registration providers
        return new[] { GetType() };
    }

    protected override void RegisterWithSailfish(ContainerBuilder builder)
    {
        switch (Environment.GetEnvironmentVariable("environment")?.ToLowerInvariant())
        {
            // These registrations can override the default handlers for
            // writing t-test results and reading/writing tracking files
            // This is useful if you've got a system for running automated perf
            // tests that record data to the cloud or some other non-default target.
            case "notify":
                builder.RegisterType<CustomNotificationHandler>().As<INotificationHandler<NotifyOnTestResultCommand>>();
                break;
            case "cloud":
                builder.RegisterType<CloudWriter>().As<ICloudWriter>();
                builder.RegisterType<CustomWriteToCloudHandler>().As<INotificationHandler<WriteCurrentTrackingFileCommand>>();
                break;
        }

        base.RegisterWithSailfish(builder);
    }
}