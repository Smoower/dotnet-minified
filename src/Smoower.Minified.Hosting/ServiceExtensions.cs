using Microsoft.Extensions.DependencyInjection;

namespace Smoower.Minified.Hosting;

// Compact DI registration helpers over IServiceCollection. Each returns the
// collection so registrations chain, e.g. svc.scoped<IFoo,Foo>().single<Bar>();
public static class ServiceExtensions
{
    public static IServiceCollection scoped<T>(this IServiceCollection s) where T : class
        => s.AddScoped<T>();

    public static IServiceCollection scoped<TService, TImpl>(this IServiceCollection s)
        where TService : class where TImpl : class, TService
        => s.AddScoped<TService, TImpl>();

    public static IServiceCollection single<T>(this IServiceCollection s) where T : class
        => s.AddSingleton<T>();

    public static IServiceCollection single<TService, TImpl>(this IServiceCollection s)
        where TService : class where TImpl : class, TService
        => s.AddSingleton<TService, TImpl>();

    public static IServiceCollection trans<T>(this IServiceCollection s) where T : class
        => s.AddTransient<T>();

    public static IServiceCollection trans<TService, TImpl>(this IServiceCollection s)
        where TService : class where TImpl : class, TService
        => s.AddTransient<TService, TImpl>();

    // Resolution side: GetRequiredService is long and common in factories/seeders.
    public static T svc<T>(this IServiceProvider sp) where T : notnull
        => sp.GetRequiredService<T>();
}
