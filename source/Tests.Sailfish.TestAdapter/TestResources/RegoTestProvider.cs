using System.Threading.Tasks;
using Autofac;
using Sailfish.Registration;

namespace Tests.Sailfish.TestAdapter.TestResources;

public class RegoTestProvider : IProvideARegistrationCallback
{
    public async Task RegisterAsync(ContainerBuilder builder)
    {
        await Task.CompletedTask;
        builder.RegisterType<GenericDependency<AnyType>>();
    }
}