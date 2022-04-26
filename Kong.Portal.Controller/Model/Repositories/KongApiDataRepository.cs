namespace Kong.Portal.Controller.Model.Repositories;

public interface IKongApiDataRepository
{
    IList<KongApiData> GetAll();
    IList<KongApiData> GetAll(string nameSpace);
    void Delete(KongApiData data);
    void Insert(KongApiData data);
}

public class KongApiDataRepository : IKongApiDataRepository
{
    private Lazy<KubernetesClientConfiguration> _config;

    public KongApiDataRepository()
    {
        _config = new Lazy<KubernetesClientConfiguration>(() => KubernetesClientConfiguration.IsInCluster()
            ? KubernetesClientConfiguration.InClusterConfig()
            : KubernetesClientConfiguration.BuildDefaultConfig());
    }
    
    public IList<KongApiData> GetAll()
    {
        var host = _config.Value.Host;

        var apis = host.AppendPathSegment($"/apis/henrik.dk/v1/kong-api-data")
            .WithOAuthBearerToken(_config.Value.AccessToken)
            .GetJsonAsync().Result;

        var result = new List<KongApiData>();
        foreach (var api in apis.items)
        {
            result.Add(new KongApiData
            {
                Name = api.metadata.name,
                NameSpace = (string)((IDictionary<string, object>)api.metadata)["namespace"],
                Data = api.spec.json,
                Updated = api.spec.updated,
            });
        }

        return result;
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
                NameSpace = (string)((IDictionary<string, object>)api.metadata)["namespace"],
                Data = api.spec.json,
                Updated = api.spec.updated,
            });
        }

        return result;
    }

    public void Delete(KongApiData data)
    {
        var host = _config.Value.Host;

        var result = host.AppendPathSegment($"/apis/henrik.dk/v1/namespaces/{data.NameSpace}/kong-api-data/{data.Name}")
            .AllowHttpStatus("4xx")
            .WithOAuthBearerToken(_config.Value.AccessToken)
            .DeleteAsync().Result;
    }

    public void Insert(KongApiData data)
    {
        var host = _config.Value.Host;

        var result = host.AppendPathSegment($"/apis/henrik.dk/v1/namespaces/{data.NameSpace}/kong-api-data")
            .WithOAuthBearerToken(_config.Value.AccessToken)
            .PostJsonAsync(new
            {
                apiVersion = "henrik.dk/v1",
                kind = "KongApiData",
                metadata = new
                {
                    name = data.Name,
                    Namespace = data.NameSpace
                },
                spec = new
                {
                    data.Data,
                    data.Updated,
                }
            }).Result;
    }
}