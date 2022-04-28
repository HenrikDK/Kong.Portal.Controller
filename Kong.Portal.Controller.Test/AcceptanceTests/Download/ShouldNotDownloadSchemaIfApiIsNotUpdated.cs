using Kong.Portal.Controller.Model;
using Kong.Portal.Controller.Model.Extensions;
using Kong.Portal.Controller.Model.Repositories;
using Kong.Portal.Controller.Reconciliation;
using Kong.Portal.Controller.Reconciliation.Cleanup;
using Kong.Portal.Controller.Reconciliation.Merge;

namespace Kong.Portal.Controller.Test.AcceptanceTests.Download;

public class ShouldNotDownloadSchemaIfApiIsNotUpdated : AcceptanceTest
{
    private KongApi _api;
    private string _json;
    private IApiSwaggerRepository _swaggerRepository;

    public ShouldNotDownloadSchemaIfApiIsNotUpdated()
    {
        _registry.AddSingleton(Substitute.For<ICleanupClusterApis>());
        _registry.AddSingleton(Substitute.For<IMergeOpenApiSchemas>());
        
        _registry.AddSingleton(Substitute.For<IApiPodRepository>());
        _registry.AddSingleton(Substitute.For<IKongRepository>());
        _registry.AddSingleton(Substitute.For<IKongApiDataRepository>());
        _registry.AddSingleton(Substitute.For<IKongApiConfigRepository>());
    }
    
    public void GivenAnApiWasDeployedEarlier()
    {
        _api = new KongApi
        {
            Name = "test-api",
            Port = 8080,
            Swagger = "/swagger.json",
            NameSpace = "test-ns"
        };
        
        var apiRepository = Substitute.For<IKongApiRepository>();
        apiRepository.GetAll().Returns(new List<KongApi> {_api});
        _registry.AddSingleton(apiRepository);
    }
    
    public void AndGivenTheApiHasBeenUpdated()
    {
        var podRepository = Substitute.For<IApiPodRepository>();
        var pod = new ApiPod
        {
            Name = _api.Name,
            NameSpace = _api.NameSpace,
            LastUpdated = DateTime.Now.AddHours(-3)
        };

        podRepository.GetAll(_api.NameSpace).Returns(new List<ApiPod> {pod});
        _registry.AddSingleton(podRepository);

        var state = new List<ApiEntry> {new() {Name = _api.Name, LastUpdated = DateTime.Now}};
        var data = new KongApiData
        {
            Name = "namespace-state",
            NameSpace = _api.NameSpace,
            Updated = DateTime.Now.AddMinutes(-1),
            Data = JsonConvert.SerializeObject(state).ToBrotliBase64()
        };
        var dataRepository = Substitute.For<IKongApiDataRepository>();
        dataRepository.GetAll(_api.NameSpace).Returns(new List<KongApiData> {data});
        _registry.AddSingleton(dataRepository);
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

    public void ThenTheServiceIsNotContacted()
    {
        _swaggerRepository.DidNotReceive().GetSwaggerJson(_api);
    }
}