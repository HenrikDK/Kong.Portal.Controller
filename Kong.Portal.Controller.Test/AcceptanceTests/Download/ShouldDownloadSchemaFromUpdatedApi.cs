using Kong.Portal.Controller.Model;
using Kong.Portal.Controller.Model.Extensions;
using Kong.Portal.Controller.Model.Repositories;
using Kong.Portal.Controller.Reconciliation.Cleanup;
using Kong.Portal.Controller.Reconciliation.Merge;

namespace Kong.Portal.Controller.Test.AcceptanceTests.Download;

public class ShouldDownloadSchemaFromUpdatedApi : AcceptanceTest
{
    private KongApi _api;
    private string _json;
    private IApiSwaggerRepository _swaggerRepository;

    public ShouldDownloadSchemaFromUpdatedApi()
    {
        _registry.AddSingleton(Substitute.For<ICleanupClusterApis>());
        _registry.AddSingleton(Substitute.For<IMergeOpenApiSchemas>());
        
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
            LastUpdated = DateTime.Now
        };

        podRepository.GetAll(_api.NameSpace).Returns(new List<ApiPod> {pod});
        _registry.AddSingleton(podRepository);

        var state = new List<ApiEntry> {new() {Name = _api.Name, LastUpdated = DateTime.Now.AddHours(-3)}};
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
        _container = new Container(_registry);
        var host = _container.GetInstance<ServiceHost>();
        host.StartAsync(_tokenSource.Token).Wait();
        Thread.Sleep(500);
        _tokenSource.Cancel();
        host.StopAsync(_tokenSource.Token);
    }
    
    public void ThenTheServiceSchemaIsDownloaded()
    {
        _swaggerRepository.Received().GetSwaggerJson(_api);
    }
        
    public void AndThenTheServiceIsUpdated()
    {
        var kongRepository = _container.GetInstance<IKongRepository>();
        kongRepository.Received().Update(_api.Name, _api.NameSpace, Arg.Any<string>());
    }
}