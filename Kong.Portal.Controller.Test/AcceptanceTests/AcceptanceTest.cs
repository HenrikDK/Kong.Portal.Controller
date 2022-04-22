using Kong.Portal.Controller.Infrastructure;
using Microsoft.AspNetCore.Connections;

namespace Kong.Portal.Controller.Test.AcceptanceTests;

public class AcceptanceTest
{
    protected CancellationTokenSource _tokenSource = new CancellationTokenSource();

    protected WorkerRegistry _registry;
    protected Container _container;

    public AcceptanceTest()
    {
        _registry = new WorkerRegistry();

        MockLogging();
        MockMemoryCache();
    }
    
    private void MockLogging()
    {
        _registry.AddSingleton(Substitute.For<ILogger>());
        _registry.AddSingleton(Substitute.For<ILogger<ServiceHost>>());
    }
        
    private void MockMemoryCache()
    {
        _registry.AddSingleton(Substitute.For<IMemoryCache>());
    }
}