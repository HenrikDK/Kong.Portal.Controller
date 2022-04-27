using Kong.Portal.Controller.Model;
using Kong.Portal.Controller.Model.Extensions;
using Kong.Portal.Controller.Model.Repositories;

namespace Kong.Portal.Controller.Test.AcceptanceTests.Merge;

public class ApiShouldBeRenamedAfterMerge : AcceptanceTest
{
    private KongApiData _api;
    private string _json = @"
{
  ""openapi"": ""3.0.1"",
  ""info"": { ""title"": ""test"", ""version"": ""v1"" },
  ""paths"": {
    ""/v1/deploy"": {},
    ""/merged"": {}
  },
  ""components"": { ""schemas"": {} }
}";
    
    private KongApiConfig _config;
    private string _mergedJson;
    private IKongApiDataRepository _dataRepository;

    public ApiShouldBeRenamedAfterMerge()
    {
        _registry.AddSingleton(Substitute.For<IApiPodRepository>());
        _registry.AddSingleton(Substitute.For<IApiSwaggerRepository>());
        _registry.AddSingleton(Substitute.For<IKongRepository>());
        _registry.AddSingleton(Substitute.For<IKongApiRepository>());

        _dataRepository = Substitute.For<IKongApiDataRepository>();
        _dataRepository.Insert(Arg.Do<KongApiData>(x => _mergedJson = x.Data.FromBrotliBase64()));
        _registry.AddSingleton(_dataRepository);
    }
        
    public void GivenAServicesHasBeenDeployed()
    {
        _api = new KongApiData
        {
            Name = "test-service",
            NameSpace = "test-ns",
            Updated = DateTime.Now,
            Data = _json.ToBrotliBase64(),
        };

        _dataRepository.GetAll().Returns(new List<KongApiData> {_api});
    }

    public void AndGivenAMergedApiHasBeenConfigured()
    {
        var configRepository = Substitute.For<IKongApiConfigRepository>();
        _config = new KongApiConfig{
            Description = "This is a very important test Api",
            Title = "Test Api",
            ContactEmail = "spam@real.com",
            LicenseName = "Test License 2.0",
            LicenseUrl = "www.test.com",
            TermsUrl = "reddit.com",
            SecurityScheme = "ApiKey",
            SecurityKeyName = "X-TEST-KEY",
            Name = "api",
            NameSpace = "test-ns"
        };
            
        configRepository.GetFirstIn(_config.NameSpace).Returns(_config);
        _registry.AddSingleton(configRepository);
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

    public void ThenTheNewApiShouldHaveTheConfiguredTitle()
    {
        _mergedJson.Contains(_config.Title).Should().BeTrue();
        _mergedJson.Contains(_config.Description).Should().BeTrue();
    }
}