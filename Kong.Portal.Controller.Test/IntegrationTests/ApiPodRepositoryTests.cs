using Kong.Portal.Controller.Model;

namespace Kong.Portal.Controller.Test.IntegrationTests;

public class ApiPodRepositoryTests
{
    private Container _container;
    private IConfiguration _configuration;

    [SetUp]
    public void Setup()
    {
        var config = KubernetesClientConfiguration.IsInCluster()
            ? KubernetesClientConfiguration.InClusterConfig()
            : KubernetesClientConfiguration.BuildDefaultConfig();

        FlurlHttp.ConfigureClient(config.Host,
            cli => cli.Settings.HttpClientFactory = new UntrustedHttpsClientFactory());
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

/*        [Test]
        public void Should_save_service_deployment()
        {
            var getDeploymentCount = _container.GetInstance<IGetDeploymentCount>();

            var count =getDeploymentCount.Execute();

            count.Should().BeGreaterThan(0);
        }
        
        [Test]
        public void Should_get_active_services()
        {
            var getActiveServices = _container.GetInstance<IGetActiveServices>();

            var services = getActiveServices.Execute();

            services.Count.Should().BeGreaterThan(0);
        }
        
        [Test]
        public void Should_get_configuration()
        {
            var getConfiguration = _container.GetInstance<IGetConfiguration>();

            var configuration = getConfiguration.Execute();

            configuration.Should().NotBeNull();
        }*/
}