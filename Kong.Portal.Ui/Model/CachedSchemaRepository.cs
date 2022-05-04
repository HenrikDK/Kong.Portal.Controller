namespace Kong.Portal.Ui.Model;

public interface ICachedSchemaRepository
{
    string GetSchema();
}

public class CachedSchemaRepository : ICachedSchemaRepository
{
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private Lazy<KubernetesClientConfiguration> _config;

    public CachedSchemaRepository(IConfiguration configuration, IMemoryCache cache)
    {
        _configuration = configuration;
        _cache = cache;
        _config = new Lazy<KubernetesClientConfiguration>(() => KubernetesClientConfiguration.IsInCluster()
            ? KubernetesClientConfiguration.InClusterConfig()
            : KubernetesClientConfiguration.BuildDefaultConfig());
    }
    
    public string GetSchema()
    {
        return _cache.GetOrCreate("json-schema", x =>
        {
            var nameSpace = _configuration.GetValue<string>("namespace");

            var all = GetAll(nameSpace);

            var kongApiData = all.FirstOrDefault(x => x.Name == "api");
        
            x.SlidingExpiration = TimeSpan.FromMinutes(2);
            return kongApiData?.Data ?? "";
        });
    }
    
    public IList<KongApiData> GetAll(string nameSpace)
    {
        var host = _config.Value.Host;

        var apis = host.AppendPathSegment($"/apis/henrik.dk/v1/namespaces/{nameSpace}/kong-api-data")
            .WithOAuthBearerToken(_config.Value.AccessToken)
            .GetJsonAsync().Result;

        var result = new List<KongApiData>();
        foreach (var api in apis.items)
        {
            result.Add(new KongApiData
            {
                Name = api.metadata.name,
                Data = api.spec.json,
            });
        }

        return result;
    }
}