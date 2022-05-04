using Kong.Portal.Controller.Infrastructure;
using Kong.Portal.Controller.Model;
using Kong.Portal.Controller.Model.Repositories;

namespace Kong.Portal.Controller.Test.IntegrationTests;

public class KongApiConfigRepositoryTests
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
            x.AssemblyContainingType<KongApiConfig>();
            x.WithDefaultConventions();
            x.LookForRegistries();
        });
        registry.AddSingleton(_configuration);
        _container = new Container(registry);
    }

    [Test]
    public void Should_get_first_object_in_cluster_namespace()
    {
        var repository = _container.GetInstance<IKongApiConfigRepository>();

        var config = repository.GetFirstIn("petstore");

        config.Should().NotBeNull();
        config.Description.Should().NotBeNullOrEmpty();
        config.Name.Should().NotBeNullOrEmpty();
        config.Title.Should().NotBeNullOrEmpty();
        config.ContactEmail.Should().NotBeNullOrEmpty();
        config.LicenseName.Should().NotBeNullOrEmpty();
        config.LicenseUrl.Should().NotBeNullOrEmpty();
        config.NameSpace.Should().NotBeNullOrEmpty();
        config.TermsUrl.Should().NotBeNullOrEmpty();
        config.SecurityScheme.Should().NotBeNullOrEmpty();
        config.SecurityKeyName.Should().BeEmpty();
    }
}