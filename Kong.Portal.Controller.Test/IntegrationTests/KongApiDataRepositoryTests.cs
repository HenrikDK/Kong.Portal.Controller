using System;
using System.Linq;
using Kong.Portal.Controller.Model;
using Kong.Portal.Controller.Model.Repositories;

namespace Kong.Portal.Controller.Test.IntegrationTests;

public class KongApiDataRepositoryTests
{
    private Container _container;
    private IConfiguration _configuration;

    [SetUp]
    public void Setup()
    {
        var config = KubernetesClientConfiguration.IsInCluster()
            ? KubernetesClientConfiguration.InClusterConfig()
            : KubernetesClientConfiguration.BuildDefaultConfig();

        FlurlHttp.ConfigureClient(config.Host, cli => cli.Settings.HttpClientFactory = new UntrustedHttpsClientFactory());
        FlurlHttp.Configure(c =>
        {
            c.JsonSerializer = new NewtonsoftJsonSerializer(new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        });

        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var registry = new ServiceRegistry();
        registry.Scan(x =>
        {
            x.AssemblyContainingType<KongApiData>();
            x.WithDefaultConventions();
            x.LookForRegistries();
        });
        registry.AddSingleton(_configuration);
        _container = new Container(registry);
    }

    //[Test]
    public void Should_get_all_kong_api_jsons_in_namespace()
    {
        var repository = _container.GetInstance<IKongApiDataRepository>();

        var jsons = repository.GetAll("petstore");

        jsons.Count.Should().BeGreaterThan(0);
    }
    
    //[Test]
    public void Should_delete_specific_kong_api_json()
    {
        var repository = _container.GetInstance<IKongApiDataRepository>();
        var json = new KongApiData
        {
            Name = "petstore-user-api",
            NameSpace = "petstore"
        };
        
        repository.Delete(json);
        
        var jsons = repository.GetAll("petstore");
        jsons.FirstOrDefault(x => x.Name == json.Name).Should().BeNull();
    }
    
    //[Test]
    public void Should_create_specific_kong_api_json()
    {
        var repository = _container.GetInstance<IKongApiDataRepository>();
        var json = new KongApiData
        {
            Name = "petstore-store-api",
            NameSpace = "petstore",
            Data = "test test",
            Updated = DateTime.Now,
        };
        repository.Delete(json);
        
        repository.Insert(json);
        
        var jsons = repository.GetAll("petstore");
        jsons.FirstOrDefault(x => x.Name == json.Name).Should().NotBeNull();
    }
}