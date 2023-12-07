﻿using Sailfish.Attributes;
using System;

namespace PerformanceTests.ExamplePerformanceTests.NonDiscoverable;

[Sailfish(Disabled = true)]
public class FaultyTestStructureNotFoundExample
{
    [SailfishVariable(1, 2, 3)] public int Variable { get; set; }

    // [SailfishMethod]
    public void ILackASailfishMethodAttribute()
    {
        Console.WriteLine("TEST");
    }
}