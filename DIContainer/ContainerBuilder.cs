namespace DIContainer;

public class ContainerBuilder : IContainerBuilder
{
    private readonly List<ServiceDescriptor> _descriptors = new();
    private readonly IActivationBuilder _builder;

    public ContainerBuilder(IActivationBuilder builder)
    {
        _builder = builder;
    }

    public void Register(ServiceDescriptor descriptor)
    {
        if (!_descriptors.Select(x => x.ServiceType).Contains(descriptor.ServiceType))
        {
            _descriptors.Add(descriptor);
        }
        else
        {
            Console.WriteLine($"\nAlready have the same key: ({descriptor.ServiceType}) skip it\n");
        }
    }
    public IContainer Build()
    {
        return new Container(_descriptors, _builder);
    }
}