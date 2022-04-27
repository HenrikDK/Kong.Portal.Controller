using Kong.Portal.Controller.Infrastructure;
using Kong.Portal.Controller.Reconciliation;
using Kong.Portal.Controller.Reconciliation.Merge;
using Kong.Portal.Controller.Reconciliation.Update;

namespace Kong.Portal.Controller.Test.AcceptanceTests;

public class AcceptanceTest
{
    protected CancellationTokenSource _tokenSource = new CancellationTokenSource();

    protected WorkerRegistry _registry;
    protected Container _container;
    private IConfiguration _configuration;

    public AcceptanceTest()
    {
        _registry = new WorkerRegistry();
        _registry.Scan(x =>
        {
            x.AssemblyContainingType<ServiceHost>();
            x.WithDefaultConventions();
            x.LookForRegistries();
        });

        MockConfiguration();
        MockLogging();
        MockMemoryCache();
    }

    private void MockConfiguration()
    {
        _configuration = Substitute.For<IConfiguration>();
        _registry.AddSingleton(_configuration);
    }

    private void MockLogging()
    {
        _registry.AddSingleton(Substitute.For<ILogger>());
        _registry.AddSingleton(Substitute.For<ILogger<ServiceHost>>());
        _registry.AddSingleton(Substitute.For<ILogger<ReconciliationScheduler>>());
        _registry.AddSingleton(Substitute.For<ILogger<ApiReconciliation>>());
        _registry.AddSingleton(Substitute.For<ILogger<UpdateClusterApis>>());
        _registry.AddSingleton(Substitute.For<ILogger<MergeClusterApis>>());
        _registry.AddSingleton(Substitute.For<ILogger<MergeOpenApiSchemas>>());
    }
        
    private void MockMemoryCache()
    {
        _registry.AddSingleton(Substitute.For<IMemoryCache>());
    }
}