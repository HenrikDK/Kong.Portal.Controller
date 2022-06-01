using Kong.Portal.Ui.Infrastructure;

namespace Kong.Portal.Ui.Model;

public interface ICachedSchemaRepository
{
    string GetSchema();
}

public class CachedSchemaRepository : ICachedSchemaRepository
{
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly IK8sClient _client;

    public CachedSchemaRepository(IConfiguration configuration, IMemoryCache cache, IK8sClient client)
    {
        _configuration = configuration;
        _cache = cache;
        _client = client;
    }
    
    public string GetSchema()
    {
        return _cache.GetOrCreate("json-schema", x =>
        {
            var nameSpace = _configuration.GetValue<string>("namespace");

            var all = GetAll(nameSpace);

            var kongApiData = all.FirstOrDefault(x => x.Name == "api");
        
            x.SlidingExpiration = TimeSpan.FromMinutes(2);
            return kongApiData?.Data.FromBrotliBase64() ?? "";
        });
    }
    
    public IList<KongApiData> GetAll(string nameSpace)
    {
        var host = _client.Host;

        var apis = host.AppendPathSegment($"/apis/henrik.dk/v1/namespaces/{nameSpace}/kong-api-data")
            .WithOAuthBearerToken(_client.AccessToken)
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