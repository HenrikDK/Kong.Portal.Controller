using Kong.Portal.Controller.Infrastructure;
using Kong.Portal.Controller.Model;
using Kong.Portal.Controller.Model.Repositories;

namespace Kong.Portal.Controller.Test.IntegrationTests;

public class ApiPodRepositoryTests
{
    private Container _container;
    private IConfiguration _configuration;

    [SetUp]
    public void Setup()
    {
        FlurlHttp.ConfigureClient(K8sClient.Server, cli => cli.Settings.HttpClientFactory = new UntrustedHttpsClientFactory());
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
            x.AssemblyContainingType<ApiPod>();
            x.WithDefaultConventions();
            x.LookForRegistries();
        });
        registry.AddSingleton(_configuration);
        _container = new Container(registry);
    }

    //[Test]
    public void Should_get_api_pods_from_cluster()
    {
        var repository = _container.GetInstance<IApiPodRepository>();

        var pods = repository.GetAll("petstore");

        pods.Count.Should().BeGreaterThan(0);
    }
}