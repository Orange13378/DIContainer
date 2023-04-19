using Autofac;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using DIContainer;
using Microsoft.Extensions.DependencyInjection;
using ContainerBuilder = DIContainer.ContainerBuilder;

IContainerBuilder builder2 = new ContainerBuilder(new ReflectionBasedActivationBuilder());
IContainerBuilder builder = new ContainerBuilder(new LambdaBasedActivationBuilder());

var container = builder
    .RegisterTransient<IService, Service>()
    .RegisterTransient<IService2>(x => new Service2())
    .RegisterScoped<Controller, Controller>()
    .RegisterScoped<Controller, Controller>()
    .RegisterScoped<IHelper>(x => new Helper())
    .RegisterSingleton<IAnotherService>(AnotherServiceInstance.Instance)
    .RegisterSingleton<IAnotherService>(x => AnotherServiceInstance.Instance)
    .Build();

var container2 = builder2
    .RegisterTransient(typeof(Controller), typeof(Controller))
    .RegisterTransient(typeof(IHelper), x => new Helper())
    .RegisterScoped(typeof(IService), typeof(Service))
    .RegisterScoped(typeof(IService), x => new Service())
    .RegisterScoped(typeof(IService2), typeof(Service2))
    .RegisterSingleton(typeof(IAnotherService), x => AnotherServiceInstance.Instance)
    .RegisterSingleton(typeof(IAnotherService), AnotherServiceInstance.Instance)
    .Build();

var scope1 = container.CreateScope();
var scope2 = container2.CreateScope();

var controller1 = scope1.Resolve(typeof(Controller));
var controller2 = scope1.Resolve(typeof(IHelper));

var controller3 = scope1.Resolve(typeof(IService));
var controller4 = scope2.Resolve(typeof(IService));

var controller5 = scope1.Resolve(typeof(IAnotherService));
var controller6 = scope2.Resolve(typeof(IAnotherService));

if (controller1 != controller2)
    Console.WriteLine("NOT The Same Types");

if (controller3 != controller4)
    Console.WriteLine("NOT The Same Scope");

if (controller5 == controller6)
    Console.WriteLine("The Same Instance");


//Benchmark (Create new, reflection, lambda(Expression Tree), AutoFac, MS DI)
//only release config

//BenchmarkRunner.Run<ContainerBenchmark>();

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

public class Controller
{
    private readonly IService _service;

    public Controller(IService service)
    {
        _service = service;
    }
}

//Empty classes just for test
public class AnotherServiceInstance
{
    private AnotherServiceInstance() {}

    public static AnotherServiceInstance Instance = new();
}
public interface IAnotherService
{

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

public interface IService2
{

}

public class Service2 : IService2
{

}
