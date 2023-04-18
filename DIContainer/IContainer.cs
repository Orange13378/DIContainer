namespace DIContainer;

public interface IContainer : IDisposable, IAsyncDisposable
{
    IScope CreateScope();
}