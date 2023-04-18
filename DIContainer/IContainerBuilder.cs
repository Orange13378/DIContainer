namespace DIContainer;

public interface IContainerBuilder
{
    void Register(ServiceDescriptor descriptor);

    IContainer Build();
}