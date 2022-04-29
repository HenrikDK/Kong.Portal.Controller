using Kong.Portal.Controller.Model;
using Kong.Portal.Controller.Model.Extensions;
using Kong.Portal.Controller.Model.Repositories;
using Kong.Portal.Controller.Reconciliation;
using Kong.Portal.Controller.Reconciliation.Cleanup;
using Kong.Portal.Controller.Reconciliation.Update;

namespace Kong.Portal.Controller.Test.AcceptanceTests.Merge;

public class ShouldSetValuesInMergedSpecBasedOnConfiguration : AcceptanceTest
{
    private KongApi _firstApi;
    private string _firstJson = @"
{
  ""openapi"": ""3.0.1"",
  ""info"": { ""title"": ""primary"", ""version"": ""v1"" },
  ""paths"": {
    ""/v1/existing"": {},
  },
  ""components"": { ""schemas"": {} }
}";
    
    private KongApi _secondApi;
    private string _secondJson = @"
{
  ""openapi"": ""3.0.1"",
  ""info"": { ""title"": ""test"", ""version"": ""v1"" },
  ""paths"": {
    ""/v1/deploy"": {},
    ""/v2/merged"": {}
  },
  ""components"": { ""schemas"": {} }
}";

    private string _json;
    private string _ingressSuffix = "henrik.dk";
    private IApiSwaggerRepository _swaggerRepository;
    private KongApiConfig _config;

    public ShouldSetValuesInMergedSpecBasedOnConfiguration()
    {
        _registry.AddSingleton(Substitute.For<ICleanupClusterApis>());
        _registry.AddSingleton(Substitute.For<IUpdateClusterApis>());

        var dataRepository = Substitute.For<IKongRepository>();
        dataRepository.Update(Arg.Any<string>(), Arg.Any<string>(), Arg.Do<string>(x => _json = x));
        _registry.AddSingleton(dataRepository);
    }
        
    public void GivenAnApiHasBeenUpdatedInTheCluster()
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
        apiRepository.GetAll().Returns(new List<KongApi> {_firstApi, _secondApi});
        _registry.AddSingleton(apiRepository);

        var firstPod = new ApiPod
        {
            Name = _firstApi.Name,
            NameSpace = _firstApi.NameSpace,
            LastUpdated = DateTime.Now.AddHours(-3)
        };
        var secondPod = new ApiPod
        {
            Name = _secondApi.Name,
            NameSpace = _secondApi.NameSpace,
            LastUpdated = DateTime.Now
        };
        var podRepository = Substitute.For<IApiPodRepository>();
        podRepository.GetAll(_firstApi.NameSpace).Returns(new List<ApiPod> {firstPod, secondPod});
        _registry.AddSingleton(podRepository);
        
        var firstData = new KongApiData
        {
            Name = "first-api",
            NameSpace = "test-ns",
            Data = _firstJson.ToBrotliBase64(),
            Updated = DateTime.Now
        };
        
        var secondData = new KongApiData
        {
            Name = "second-api",
            NameSpace = "test-ns",
            Data = _secondJson.ToBrotliBase64(),
            Updated = DateTime.Now.AddHours(-2)
        };
        
        var dataRepository = Substitute.For<IKongApiDataRepository>();
        dataRepository.GetAll(_firstApi.NameSpace).Returns(new List<KongApiData> {firstData, secondData});
        _registry.AddSingleton(dataRepository);
    }
    
    public void AndGivenTheNamespaceIsConfiguredForMergeApi()
    {
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
        var configRepository = Substitute.For<IKongApiConfigRepository>();
        configRepository.GetFirstIn(_config.NameSpace).Returns(_config);
        _registry.AddSingleton(configRepository);
    }

    public void AndGivenTheServiceIsRunning()
    {
        _configuration.Add("ingress-suffix", _ingressSuffix);
        _json = @"{ ""test_json"":{} }";

        _swaggerRepository = Substitute.For<IApiSwaggerRepository>();
        _swaggerRepository.GetSwaggerJson(null).ReturnsForAnyArgs(_json);
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

    public void ThenTheMergedSpecContainsValuesProvidedInConfiguration()
    {
        var spec = JObject.Parse(_json);
        spec["servers"]["url"].ToString().Should().Be($"https://api.{_firstApi.NameSpace}.{_ingressSuffix}");
        
        spec["info"]["title"].ToString().Should().Be(_config.Title);
        spec["info"]["description"].ToString().Should().Be(_config.Description);

        spec["info"]["termsOfService"].ToString().Should().Be(_config.TermsUrl);
        
        spec["info"]["contact"]["email"].ToString().Should().Be(_config.ContactEmail);

        spec["info"]["license"]["name"].ToString().Should().Be(_config.LicenseName);
        spec["info"]["license"]["url"].ToString().Should().Be(_config.LicenseUrl);
        
        spec["components"]["securitySchemes"]["ApiKeyAuth"]["type"].ToString().Should().Be("apiKey");
        spec["components"]["securitySchemes"]["ApiKeyAuth"]["name"].ToString().Should().Be(_config.SecurityKeyName);
    }
}