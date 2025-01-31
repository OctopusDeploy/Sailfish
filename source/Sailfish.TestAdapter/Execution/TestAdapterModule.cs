﻿using Autofac;
using MediatR;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Sailfish.Analysis.SailDiff;
using Sailfish.Analysis.ScaleFish;
using Sailfish.Contracts.Private.ExecutionCallbackHandlers;
using Sailfish.TestAdapter.FrameworkHandlers;

namespace Sailfish.TestAdapter.Execution;

internal class TestAdapterModule(IFrameworkHandle? frameworkHandle) : Module
{
    private readonly IFrameworkHandle? frameworkHandle = frameworkHandle;

    protected override void Load(ContainerBuilder builder)
    {
        if (frameworkHandle is not null) builder.RegisterInstance(frameworkHandle).As<IFrameworkHandle>();

        builder.RegisterType<TestAdapterExecutionProgram>().As<ITestAdapterExecutionProgram>();
        builder.RegisterType<TestAdapterExecutionEngine>().As<ITestAdapterExecutionEngine>();
        builder.RegisterType<AdapterConsoleWriter>().As<IAdapterConsoleWriter>();
        builder.RegisterType<AdapterSailDiff>().As<ISailDiffInternal>().InstancePerDependency();
        builder.RegisterType<AdapterSailDiff>().As<IAdapterSailDiff>();
        builder.RegisterType<AdapterScaleFish>().As<IScaleFishInternal>();
        builder.RegisterType<AdapterScaleFish>().As<IAdapterScaleFish>();

        builder.RegisterType<ExecutionStartingNotificationHandler>().As<INotificationHandler<ExecutionStartingNotification>>();
        builder.RegisterType<ExecutionCompletedNotificationHandler>().As<INotificationHandler<ExecutionCompletedNotification>>();
        builder.RegisterType<ExecutionDisabledNotificationHandler>().As<INotificationHandler<ExecutionDisabledNotification>>();
        builder.RegisterType<ExceptionNotificationHandler>().As<INotificationHandler<ExceptionNotification>>();


    }
}