using Microsoft.Extensions.DependencyInjection;
using Smoower.Minified.Hosting;
using Xunit;

namespace Smoower.Minified.Tests;

public class ServiceExtensionsTests
{
    private interface IFoo;
    private sealed class Foo : IFoo;
    private sealed class Bar;

    [Fact]
    public void Scoped_RegistersWithScopedLifetime()
    {
        var services = new ServiceCollection().scoped<IFoo, Foo>();
        var d = Assert.Single(services);
        Assert.Equal(ServiceLifetime.Scoped, d.Lifetime);
        Assert.Equal(typeof(IFoo), d.ServiceType);
        Assert.Equal(typeof(Foo), d.ImplementationType);
    }

    [Fact]
    public void Single_ResolvesSameInstance()
    {
        var provider = new ServiceCollection().single<Bar>().BuildServiceProvider();
        Assert.Same(provider.GetRequiredService<Bar>(), provider.GetRequiredService<Bar>());
    }

    [Fact]
    public void Trans_ResolvesNewInstances()
    {
        var provider = new ServiceCollection().trans<Bar>().BuildServiceProvider();
        Assert.NotSame(provider.GetRequiredService<Bar>(), provider.GetRequiredService<Bar>());
    }

    [Fact]
    public void Svc_ResolvesRequiredService()
    {
        IServiceProvider provider = new ServiceCollection().single<Bar>().BuildServiceProvider();
        Assert.Same(provider.GetRequiredService<Bar>(), provider.svc<Bar>());
    }
}
