using Autofac;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using DIContainer;
using Microsoft.Extensions.DependencyInjection;
using ContainerBuilder = DIContainer.ContainerBuilder;
using IContainer = DIContainer.IContainer;

IContainerBuilder builder = new ContainerBuilder(new LambdaBasedActivationBuilder());
var container = builder
    .RegisterTransient<IService, Service>()
    .RegisterScoped<Controller, Controller>()
    .RegisterSingleton<IAnotherService>(AnotherServiceInstance.Instance)
    .Build();

var scope = container.CreateScope();
var scope2 = container.CreateScope();
var controller1 = scope.Resolve(typeof(Controller));
var controller2 = scope.Resolve(typeof(Controller));
var controller3 = scope2.Resolve(typeof(Controller));
var i1 = scope.Resolve(typeof(IAnotherService));
var i2 = scope2.Resolve(typeof(IAnotherService));

if (i1 != i2)
{
    throw new InvalidOperationException();
}

if (controller1 != controller2)
{
    throw new InvalidOperationException();
}

if (controller3 != controller2)
{
    //throw new InvalidOperationException();
}

//Console.ReadLine();

BenchmarkRunner.Run<ContainerBenchmark>();

[MemoryDiagnoser]
public class ContainerBenchmark
{
    private readonly IScope _reflectionBased, _lambdaBased;
    private readonly ILifetimeScope _scope;
    private readonly IServiceScope _serviceScope;

    public ContainerBenchmark()
    {
        var lambdaBasedBuilder = new ContainerBuilder(new LambdaBasedActivationBuilder());
        var reflectionBasedBuilder = new ContainerBuilder(new ReflectionBasedActivationBuilder());

        InitContainer(lambdaBasedBuilder);
        InitContainer(reflectionBasedBuilder);

        _reflectionBased = reflectionBasedBuilder.Build().CreateScope();
        _lambdaBased = lambdaBasedBuilder.Build().CreateScope();
        _scope = InitAutofac();
        _serviceScope = InitMSDI();
    }

    private void InitContainer(ContainerBuilder builder)
    {
        builder.RegisterTransient<IService, Service>()
            .RegisterTransient<Controller, Controller>();
    }

    private ILifetimeScope InitAutofac()
    {
        var containerBuilder = new Autofac.ContainerBuilder();
        containerBuilder.RegisterType<Service>().As<IService>();
        containerBuilder.RegisterType<Controller>().AsSelf();
        return containerBuilder.Build().BeginLifetimeScope();
    }

    private IServiceScope InitMSDI()
    {
        var collection = new ServiceCollection();
        collection.AddTransient<IService, Service>();
        collection.AddTransient<Controller, Controller>();
        return collection.BuildServiceProvider().CreateScope();
    }

    [Benchmark(Baseline = true)]
    public Controller Create() => new Controller(new Service());
    [Benchmark]
    public Controller Reflection() => (Controller)_reflectionBased.Resolve(typeof(Controller));
    [Benchmark]
    public Controller Lambda() => (Controller)_lambdaBased.Resolve(typeof(Controller));
    [Benchmark]
    public Controller Autofac() => _scope.Resolve<Controller>();
    [Benchmark]
    public Controller MSDI() => _serviceScope.ServiceProvider.GetRequiredService<Controller>();
}


interface IAnotherService
{

}

class AnotherServiceInstance
{
    private AnotherServiceInstance() {}

    public static AnotherServiceInstance Instance = new();
}

public interface IHelper
{

}

public class Helper : IHelper
{

}

public interface IService
{

}

public class Service : IService
{

}


class Registration
{
    public IContainer ConfigureService()
    {
        var builder = new ContainerBuilder(new LambdaBasedActivationBuilder());
        builder.RegisterTransient<IService, Service>();
        builder.RegisterScoped<Controller, Controller>();
        return builder.Build();
    }
}

public class Controller
{
    private readonly IService _service;

    public Controller(IService service)
    {
        _service = service;
    }

    public void Do()
    {

    }
}
