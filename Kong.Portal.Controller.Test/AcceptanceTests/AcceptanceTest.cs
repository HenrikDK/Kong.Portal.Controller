using Kong.Portal.Controller.Infrastructure;
using Kong.Portal.Controller.Reconciliation;
using Kong.Portal.Controller.Reconciliation.Cleanup;
using Kong.Portal.Controller.Reconciliation.Merge;
using Kong.Portal.Controller.Reconciliation.Update;

namespace Kong.Portal.Controller.Test.AcceptanceTests;

public class AcceptanceTest
{
    protected CancellationTokenSource _tokenSource = new CancellationTokenSource();
    protected Dictionary<string, string> _configuration = new();

    protected ServiceRegistry _registry;
    protected Container _container;

    public AcceptanceTest()
    {
        _registry = new ServiceRegistry();
        _registry.Scan(x =>
        {
            x.AssemblyContainingType<ServiceHost>();
            x.WithDefaultConventions();
            x.LookForRegistries();
        });

        MockLogging();
        MockMemoryCache();
    }

    protected void BuildContainer()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(_configuration)
            .Build();
        _registry.AddSingleton((IConfiguration) configuration);

        _container = new Container(_registry);
    }

    private void MockLogging()
    {
        _registry.AddSingleton(Substitute.For<ILogger>());
        _registry.AddSingleton(Substitute.For<ILogger<ServiceHost>>());
        _registry.AddSingleton(Substitute.For<ILogger<ReconciliationScheduler>>());
        _registry.AddSingleton(Substitute.For<ILogger<ApiReconciliation>>());
        _registry.AddSingleton(Substitute.For<ILogger<CleanupClusterApis>>());
        _registry.AddSingleton(Substitute.For<ILogger<UpdateClusterApis>>());
        _registry.AddSingleton(Substitute.For<ILogger<MergeClusterApis>>());
        _registry.AddSingleton(Substitute.For<ILogger<MergeOpenApiSchemas>>());
    }
        
    private void MockMemoryCache()
    {
        _registry.AddSingleton(Substitute.For<IMemoryCache>());
    }
}