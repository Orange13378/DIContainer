namespace DIContainer;

public abstract class ServiceDescriptor
{
    public Type ServiceType { get; init; }
    public Lifetime Lifetime { get; init; }
}

public class TypeBasedServiceDescriptor : ServiceDescriptor
{
    public Type ImplementationType { get; init; }
}

public class FactoryBasedServiceDescriptor : ServiceDescriptor
{
    public Func<IScope, object> Factory { get; init; }
}

public class InstanceBasedServiceDescriptor : ServiceDescriptor
{
    public object Instance { get; init; }

    public InstanceBasedServiceDescriptor(Type serviceType, object instance)
    {
        Lifetime = Lifetime.Singleton;
        ServiceType = serviceType;
        Instance = instance;
    }
}
