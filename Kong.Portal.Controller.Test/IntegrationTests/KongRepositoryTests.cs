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
    public void Should_update_api_in_kong_developer_portal()
    {
        var repository = _container.GetInstance<IKongRepository>();

        var swagger = File.ReadAllText("/data/swagger.json");

        var json = JObject.Parse(swagger);

        var url = new JObject();
        url["url"] = $"https://api.petstore.henrik.dk";
        var array = new JArray();
        array.Add(url);
        json["servers"] = array;

        repository.Update("api", "petstore", json.ToString());
    }
}