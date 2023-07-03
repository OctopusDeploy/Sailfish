using Sailfish.Attributes;
using Shouldly;

namespace Tests.E2ETestSuite.Discoverable;

[Sailfish]
public class PrivateSettersShouldError
{
    public int PrivateSetter { get; private set; }

    [SailfishVariable(1, 2, 3)] public int Placeholder { get; set; }
    [SailfishVariable(1, 2, 3)] private int PrivatePropertyErrors { get; set; }

    [SailfishGlobalSetup]
    public void GlobalSetup()
    {
        PrivateSetter = 99;
    }

    [SailfishMethod]
    public void MainMethod()
    {
        PrivateSetter.ShouldBe(99);
    }
}