using Kong.Portal.Controller.Model;
using Kong.Portal.Controller.Model.Repositories;

namespace Kong.Portal.Controller.Test.IntegrationTests;

public class KongRepositoryTests
{
    private Container _container;
    private IConfiguration _configuration;

    [SetUp]
    public void Setup()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var registry = new ServiceRegistry();
        registry.Scan(x =>
        {
            x.AssemblyContainingType<KongApi>();
            x.WithDefaultConventions();
            x.LookForRegistries();
        });
        registry.AddSingleton(_configuration);
        _container = new Container(registry);
    }

    //[Test]
    public void Should_remove_api_from_kong()
    {
        var repository = _container.GetInstance<IKongRepository>();

        repository.Delete("api", "petstore");
    }
    
    //[Test]
    public void Should_add_api_to_kong()
    {
        var repository = _container.GetInstance<IKongRepository>();

        var swagger = File.ReadAllText("/data/swagger.json");

        repository.Update("api", "petstore", swagger);
    }
}