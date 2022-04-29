using Kong.Portal.Controller.Model;
using Kong.Portal.Controller.Model.Extensions;
using Kong.Portal.Controller.Model.Repositories;
using Kong.Portal.Controller.Reconciliation;
using Kong.Portal.Controller.Reconciliation.Cleanup;
using Kong.Portal.Controller.Reconciliation.Update;

namespace Kong.Portal.Controller.Test.AcceptanceTests.Merge;

public class ShouldDeletePersistedSpecIfApiIsNoLongerInCluster : AcceptanceTest
{
    private KongApi _firstApi;
    private KongApi _secondApi;
    private string _json;
    private IApiSwaggerRepository _swaggerRepository;

    public ShouldDeletePersistedSpecIfApiIsNoLongerInCluster()
    {
        _registry.AddSingleton(Substitute.For<ICleanupClusterApis>());
        _registry.AddSingleton(Substitute.For<IUpdateClusterApis>());
        
        _registry.AddSingleton(Substitute.For<IApiPodRepository>());
        _registry.AddSingleton(Substitute.For<IKongRepository>());
    }
        
    public void GivenAnApiHasBeenRemovedFromTheCluster()
    {
        _firstApi = new KongApi
        {
            Name = "first-api",
            NameSpace = "test-ns",
            Port = 8080,
            Swagger = "/swagger.json"
        };
        _secondApi = new KongApi
        {
            Name = "second-api",
            NameSpace = "test-ns",
            Port = 8080,
            Swagger = "/swagger.json"
        };
        var apiRepository = Substitute.For<IKongApiRepository>();
        apiRepository.GetAll().Returns(new List<KongApi> {_firstApi});
        _registry.AddSingleton(apiRepository);
        
        var firstData = new KongApiData
        {
            Name = _firstApi.Name,
            NameSpace = _firstApi.NameSpace,
            Updated = DateTime.Now,
            Data =  @"{ ""test_json"":{} }".ToBrotliBase64()
        };
        var secondData = new KongApiData
        {
            Name = _secondApi.Name,
            NameSpace = _secondApi.NameSpace,
            Updated = DateTime.Now,
            Data =  @"{ ""test_json"":{} }".ToBrotliBase64()
        };
        var dataRepository = Substitute.For<IKongApiDataRepository>();
        dataRepository.GetAll(_firstApi.NameSpace).Returns(new List<KongApiData> {firstData, secondData});
        _registry.AddSingleton(dataRepository);
    }

    public void AndGivenTheNamespaceIsConfiguredForMergeApi()
    {
        var configRepository = Substitute.For<IKongApiConfigRepository>();
        configRepository.GetFirstIn(_firstApi.NameSpace).Returns(new KongApiConfig());
        _registry.AddSingleton(configRepository);
    }
    
    public void AndGivenTheServiceIsRunning()
    {
        _json = @"{ ""test_json"":{} }";

        _swaggerRepository = Substitute.For<IApiSwaggerRepository>();
        _swaggerRepository.GetSwaggerJson(_firstApi).Returns(_json);
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

    public void ThenTheApiSpecIsRemovedFromTheCluster()
    {
        var dataRepository = _container.GetInstance<IKongApiDataRepository>();
        dataRepository.Received().Delete(Arg.Is<KongApiData>(x => x.Name == _secondApi.Name && x.NameSpace == _secondApi.NameSpace));
    }
    
    public void AndThenTheMergedApiSpecIsUpdatedInTheCluster()
    {
        var dataRepository = _container.GetInstance<IKongApiDataRepository>();
        dataRepository.Received().Insert(Arg.Is<KongApiData>(x => x.Name == "api" && x.NameSpace == _secondApi.NameSpace));
    }
}