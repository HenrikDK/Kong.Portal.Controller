using Kong.Portal.Controller.Model;
using Kong.Portal.Controller.Model.Repositories;
using Kong.Portal.Controller.Reconciliation.Merge;

namespace Kong.Portal.Controller.Test.AcceptanceTests.Merge;

public class ASingleServiceShouldBeMerged : AcceptanceTest
{
    private KongApiData _api;
    private string _json = "{'paths':{}}";
    private KongApiConfig _config;
    private string _mergedJson;
    private IKongApiConfigRepository _configRepository;

    public ASingleServiceShouldBeMerged()
    {
        _registry.AddSingleton(Substitute.For<IApiPodRepository>());
        _registry.AddSingleton(Substitute.For<IApiSwaggerRepository>());
        _registry.AddSingleton(Substitute.For<IKongRepository>());
        _registry.AddSingleton(Substitute.For<IKongApiRepository>());
        _registry.AddSingleton(Substitute.For<IMergeOpenApiSchema>());
    }
        
    public void GivenASingleServiceHasBeenDeployed()
    {
        _api = new KongApiData
        {
            Name = "test-api",
            NameSpace = "test-ns",
            Updated = DateTime.Now,
        };
        
        var dataRepository = Substitute.For<IKongApiDataRepository>();
        dataRepository.GetAll(_api.NameSpace).Returns(new List<KongApiData> {_api});
        _registry.AddSingleton(dataRepository);
    }

    public void AndGivenAMergedApiHasBeenConfigured()
    {
        _configRepository = Substitute.For<IKongApiConfigRepository>();
        _config = new KongApiConfig
        {
            Name = "api",
            NameSpace = "test-ns",

            Description = "This is a very important test Api",
            Title = "Test Api",
            ContactEmail = "spam@real.com",
            LicenseName = "Test License 2.0",
            LicenseUrl = "www.test.com",
            TermsUrl = "reddit.com",
            SecurityScheme = "ApiKey",
            SecurityKeyName = "X-TEST-KEY"
        };
            
        _configRepository.GetFirstIn(_config.NameSpace).Returns(_config);
        _registry.AddSingleton(_configRepository);
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

    public void ThenTheSchemaIsMergedIntoTemplate()
    {
        var merge = _container.GetInstance<IMergeOpenApiSchema>();
        merge.Received().Execute(Arg.Any<JObject>(), Arg.Any<KongApiData>());
    }
        
    public void AndThenTheNewSchemaIsSaved()
    {
        var dataRepository = _container.GetInstance<IKongApiDataRepository>();
        dataRepository.Received().Insert(Arg.Any<KongApiData>());
    }
}