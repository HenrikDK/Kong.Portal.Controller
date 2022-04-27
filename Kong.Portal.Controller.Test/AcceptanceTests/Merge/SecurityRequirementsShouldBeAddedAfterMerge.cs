using Kong.Portal.Controller.Model;
using Kong.Portal.Controller.Model.Extensions;
using Kong.Portal.Controller.Model.Repositories;

namespace Kong.Portal.Controller.Test.AcceptanceTests.Merge;

public class SecurityRequirementsShouldBeAddedAfterMerge : AcceptanceTest
{
    private KongApiData _firstApi;
    private string _firstJson = @"
{
  ""openapi"": ""3.0.1"",
  ""info"": { ""title"": ""primary"", ""version"": ""v1"" },
  ""paths"": {
    ""/v1/existing"": {},
  },
  ""components"": { ""schemas"": {} }
}";

    private KongApiData _secondApi;
    private string _secondJson = @"
{
  ""openapi"": ""3.0.1"",
  ""info"": { ""title"": ""test"", ""version"": ""v1"" },
  ""paths"": {
    ""/v1/deploy"": {},
    ""/merged"": {}
  },
  ""components"": { ""schemas"": {} }
}";
    
    private string _mergedJson;

    private IKongApiDataRepository _dataRepository;
    private IKongApiConfigRepository _configRepository;
    private KongApiConfig _config;
    
    public SecurityRequirementsShouldBeAddedAfterMerge()
    {
        _registry.AddSingleton(Substitute.For<IApiPodRepository>());
        _registry.AddSingleton(Substitute.For<IKongRepository>());
        _registry.AddSingleton(Substitute.For<IKongApiRepository>());

        _dataRepository = Substitute.For<IKongApiDataRepository>();
        _dataRepository.Insert(Arg.Do<KongApiData>(x => _mergedJson = x.Data.FromBrotliBase64()));
        _registry.AddSingleton(_dataRepository);
    }
        
    public void GivenTwoServicesHaveBeenDeployed()
    {
        _firstApi = new KongApiData
        {
            Name = "first-api",
            NameSpace = "test-ns",
            Updated = DateTime.Now,
            Data = _firstJson.ToBrotliBase64()
        };
        
        _secondApi = new KongApiData
        {
            Name = "second-api",
            NameSpace = "test-ns",
            Updated = DateTime.Now,
            Data = _secondJson.ToBrotliBase64()
        };

        var swaggerRepository = Substitute.For<IApiSwaggerRepository>();
        swaggerRepository.GetSwaggerJson(Arg.Is<KongApi>(x => x.Name == _firstApi.Name)).Returns(_firstJson);
        swaggerRepository.GetSwaggerJson(Arg.Is<KongApi>(x => x.Name == _secondApi.Name)).Returns(_secondJson);
        _registry.AddSingleton(swaggerRepository);

        var dataRepository = Substitute.For<IKongApiDataRepository>();
        _dataRepository.GetAll(_firstApi.NameSpace).Returns(new List<KongApiData> {_firstApi, _secondApi});
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

    public void ThenTheSecuritySetupHasBeenAddedToMergedSchema()
    {
        _mergedJson.Contains("ApiKeyAuth").Should().BeTrue();
        _mergedJson.Contains(_config.SecurityKeyName).Should().BeTrue();
    }
    
    public void AndThenANewMergedSchemaIsSaved()
    {
        _dataRepository.Received().Insert(Arg.Any<KongApiData>());
    }
}