using Kong.Portal.Controller.Model;
using Kong.Portal.Controller.Model.Repositories;
using Kong.Portal.Controller.Reconciliation;
using Kong.Portal.Controller.Reconciliation.Cleanup;
using Kong.Portal.Controller.Reconciliation.Update;

namespace Kong.Portal.Controller.Test.AcceptanceTests.Merge;

public class ShouldPersistNewInputApiSpecInCluster : AcceptanceTest
{
    private KongApi _api;
    private string _json;
    private IApiSwaggerRepository _swaggerRepository;

    public ShouldPersistNewInputApiSpecInCluster()
    {
        _registry.AddSingleton(Substitute.For<ICleanupClusterApis>());
        _registry.AddSingleton(Substitute.For<IUpdateClusterApis>());
        
        _registry.AddSingleton(Substitute.For<IApiPodRepository>());
        _registry.AddSingleton(Substitute.For<IKongRepository>());
        _registry.AddSingleton(Substitute.For<IKongApiDataRepository>());
    }
        
    public void GivenANewApiHasBeenDeployed()
    {
        _api = new KongApi
        {
            Name = "test-api",
            NameSpace = "test-ns",
            Port = 8080,
            Swagger = "/swagger.json"
        };

        var apiRepository = Substitute.For<IKongApiRepository>();
        apiRepository.GetAll().Returns(new List<KongApi> {_api});
        _registry.AddSingleton(apiRepository);
    }

    public void AndGivenTheNamespaceIsConfiguredForMergeApi()
    {
        var configRepository = Substitute.For<IKongApiConfigRepository>();
        configRepository.GetFirstIn(_api.NameSpace).Returns(new KongApiConfig());
        _registry.AddSingleton(configRepository);
    }
    
    public void AndGivenTheServiceIsRunning()
    {
        _json = @"{ ""test_json"":{} }";

        _swaggerRepository = Substitute.For<IApiSwaggerRepository>();
        _swaggerRepository.GetSwaggerJson(_api).Returns(_json);
        _registry.AddSingleton(_swaggerRepository);
    }

    public void WhenTheSystemIsRunning()
    {
        BuildContainer();
        var scheduler = _container.GetInstance<IReconciliationScheduler>();
        _tokenSource.CancelAfter(500);
        try
        {
            scheduler.RunOnInterval(_tokenSource.Token, TimeSpan.FromMinutes(2));
        }
        catch (Exception e) { }
    }

    public void ThenTheApiSpecIsPersistedInTheCluster()
    {
        var dataRepository = _container.GetInstance<IKongApiDataRepository>();
        dataRepository.Received().Insert(Arg.Is<KongApiData>(x => x.Name == _api.Name && x.NameSpace == _api.NameSpace));
    }
}