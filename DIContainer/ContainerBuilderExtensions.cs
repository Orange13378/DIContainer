namespace DIContainer;

public static class ContainerBuilderExtensions
{
    private static IContainerBuilder RegisterType(this IContainerBuilder builder, Type serviceType, Type implementationType,
        Lifetime lifetime)
    {
        if (!serviceType.IsAssignableFrom(implementationType))
        {
            throw new ArgumentException($"{implementationType} and {serviceType} doesn't Assignable types");
        }

        builder.Register(new TypeBasedServiceDescriptor()
        {
            ImplementationType = implementationType,
            ServiceType = serviceType,
            Lifetime = lifetime
        });

        return builder;
    }

    private static IContainerBuilder RegisterFactory(this IContainerBuilder builder, Type serviceType, Func<IScope, object> factory,
        Lifetime lifetime)
    {
        builder.Register(new FactoryBasedServiceDescriptor()
        {
            Factory = factory,
            ServiceType = serviceType,
            Lifetime = lifetime
        });

        return builder;
    }

    private static IContainerBuilder RegisterInstance(this IContainerBuilder builder, Type serviceType, object instance)
    {
        builder.Register(new InstanceBasedServiceDescriptor(serviceType, instance));

        return builder;
    }

    public static IContainerBuilder RegisterSingleton(this IContainerBuilder builder, Type serviceType, object instance)
        => builder.RegisterInstance(serviceType, instance);
    public static IContainerBuilder RegisterSingleton<T>(this IContainerBuilder builder, object instance)
        => builder.RegisterInstance(typeof(T), instance);
    public static IContainerBuilder RegisterSingleton(this IContainerBuilder builder, Type serviceType, Func<IScope, object> factory)
        => builder.RegisterFactory(serviceType, factory, Lifetime.Singleton);

    public static IContainerBuilder RegisterSingleton<TService>(this IContainerBuilder builder, Func<IScope, object> factory)
        => builder.RegisterFactory(typeof(TService), factory, Lifetime.Singleton);



    public static IContainerBuilder RegisterTransient(this IContainerBuilder builder, Type serviceType,
        Type serviceImplementation)
        => builder.RegisterType(serviceType, serviceImplementation, Lifetime.Transient);

    public static IContainerBuilder RegisterTransient<TService, TImplementation>(this IContainerBuilder builder) where TImplementation : TService
        => builder.RegisterType(typeof(TService), typeof(TImplementation), Lifetime.Transient);

    public static IContainerBuilder RegisterTransient(this IContainerBuilder builder, Type serviceType, Func<IScope, object> factory)
        => builder.RegisterFactory(serviceType, factory, Lifetime.Transient);

    public static IContainerBuilder RegisterTransient<TService>(this IContainerBuilder builder, Func<IScope, TService> factory)
        => builder.RegisterFactory(typeof(TService), s => factory(s), Lifetime.Transient);



    public static IContainerBuilder RegisterScoped(this IContainerBuilder builder, Type serviceType,
        Type serviceImplementation)
        => builder.RegisterType(serviceType, serviceImplementation, Lifetime.Scoped);

    public static IContainerBuilder RegisterScoped<TService, TImplementation>(this IContainerBuilder builder) where TImplementation : TService
        => builder.RegisterType(typeof(TService), typeof(TImplementation), Lifetime.Scoped);

    public static IContainerBuilder RegisterScoped(this IContainerBuilder builder, Type serviceType, Func<IScope, object> factory)
        => builder.RegisterFactory(serviceType, factory, Lifetime.Scoped);

    public static IContainerBuilder RegisterScoped<TService>(this IContainerBuilder builder, Func<IScope, TService> factory)
        => builder.RegisterFactory(typeof(TService), s => factory(s), Lifetime.Scoped);
}
