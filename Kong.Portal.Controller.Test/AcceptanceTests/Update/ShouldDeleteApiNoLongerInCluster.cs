using Kong.Portal.Controller.Model;
using Kong.Portal.Controller.Model.Extensions;
using Kong.Portal.Controller.Model.Repositories;
using Kong.Portal.Controller.Reconciliation;
using Kong.Portal.Controller.Reconciliation.Cleanup;
using Kong.Portal.Controller.Reconciliation.Merge;

namespace Kong.Portal.Controller.Test.AcceptanceTests.Update;

public class ShouldDeleteApiNoLongerInCluster : AcceptanceTest
{
    private KongApi _firstApi;
    private KongApi _secondApi;

    public ShouldDeleteApiNoLongerInCluster()
    {
        _registry.AddSingleton(Substitute.For<ICleanupClusterApis>());
        _registry.AddSingleton(Substitute.For<IMergeOpenApiSchema>());
        
        _registry.AddSingleton(Substitute.For<IApiPodRepository>());
        _registry.AddSingleton(Substitute.For<IKongRepository>());
        _registry.AddSingleton(Substitute.For<IApiSwaggerRepository>());
        _registry.AddSingleton(Substitute.For<IKongApiDataRepository>());
        _registry.AddSingleton(Substitute.For<IKongApiConfigRepository>());
    }
        
    public void GivenAnApiWasRemovedFromTheCluster()
    {
        _firstApi = new KongApi
        {
            Name = "first-api",
            Port = 8080,
            Swagger = "/swagger.json",
            NameSpace = "test-ns"
        };

        _secondApi = new KongApi
        {
            Name = "second-api",
            Port = 8080,
            Swagger = "/swagger.json",
            NameSpace = "test-ns"
        };
        
        var apiRepository = Substitute.For<IKongApiRepository>();
        apiRepository.GetAll().Returns(new List<KongApi>{_firstApi});
        _registry.AddSingleton(apiRepository);
    }

    public void AndGivenTheApiAlreadyExistsInKong()
    {
        var podRepository = Substitute.For<IApiPodRepository>();
        var pod = new ApiPod
        {
            Name = _firstApi.Name,
            NameSpace = _firstApi.NameSpace,
            LastUpdated = DateTime.Now.AddHours(-4)
        };
        podRepository.GetAll(_firstApi.NameSpace).Returns(new List<ApiPod> {pod});
        _registry.AddSingleton(podRepository);

        var state = new List<ApiEntry>
        {
            new() {Name = _firstApi.Name, LastUpdated = DateTime.Now},
            new() {Name = _secondApi.Name, LastUpdated = DateTime.Now.AddHours(-1)}
        };
        var data = new KongApiData
        {
            Name = "namespace-state",
            NameSpace = _firstApi.NameSpace,
            Updated = DateTime.Now.AddMinutes(-1),
            Data = JsonConvert.SerializeObject(state).ToBrotliBase64()
        };
        var dataRepository = Substitute.For<IKongApiDataRepository>();
        dataRepository.GetAll(_firstApi.NameSpace).Returns(new List<KongApiData> {data});
        _registry.AddSingleton(dataRepository);
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

    public void ThenTheApiIsRemovedFromTheKongDeveloperPortal()
    {
        var kongRepository = _container.GetInstance<IKongRepository>();
        kongRepository.Received().Delete(_secondApi.Name, _secondApi.NameSpace);
    }
}