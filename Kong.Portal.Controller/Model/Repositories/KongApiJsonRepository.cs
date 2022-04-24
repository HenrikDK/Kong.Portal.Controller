namespace Kong.Portal.Controller.Model.Repositories;

public interface IKongApiJsonRepository
{
    IList<KongApiJson> GetAll(string nameSpace);
    void Delete(KongApiJson json);
    void Insert(KongApiJson json);
}

public class KongApiJsonRepository : IKongApiJsonRepository
{
    private Lazy<KubernetesClientConfiguration> _config;

    public KongApiJsonRepository()
    {
        _config = new Lazy<KubernetesClientConfiguration>(() => KubernetesClientConfiguration.IsInCluster()
            ? KubernetesClientConfiguration.InClusterConfig()
            : KubernetesClientConfiguration.BuildDefaultConfig());
    }
    
    public IList<KongApiJson> GetAll(string nameSpace)
    {
        var host = _config.Value.Host;

        var apis = host.AppendPathSegment($"/apis/henrik.dk/v1/namespaces/{nameSpace}/kong-api-jsons")
            .WithOAuthBearerToken(_config.Value.AccessToken)
            .GetJsonAsync().Result;

        var result = new List<KongApiJson>();
        foreach (var api in apis.items)
        {
            result.Add(new KongApiJson
            {
                Name = api.metadata.name,
                NameSpace = (string)((IDictionary<string, object>)api.metadata)["namespace"],
                Json = api.spec.json,
                Updated = api.spec.updated,
                MergedCount = (int)api.spec.mergedCount
            });
        }

        return result;
    }

    public void Delete(KongApiJson json)
    {
        var host = _config.Value.Host;

        var result = host.AppendPathSegment($"/apis/henrik.dk/v1/namespaces/{json.NameSpace}/kong-api-jsons/{json.Name}")
            .AllowHttpStatus("4xx")
            .WithOAuthBearerToken(_config.Value.AccessToken)
            .DeleteAsync().Result;
    }

    public void Insert(KongApiJson json)
    {
        var host = _config.Value.Host;

        var result = host.AppendPathSegment($"/apis/henrik.dk/v1/namespaces/{json.NameSpace}/kong-api-jsons")
            .WithOAuthBearerToken(_config.Value.AccessToken)
            .PostJsonAsync(new
            {
                apiVersion = "henrik.dk/v1",
                kind = "KongApiJson",
                metadata = new
                {
                    name = json.Name,
                    Namespace = json.NameSpace
                },
                spec = new
                {
                    json.Json,
                    json.Updated,
                    json.MergedCount
                }
            }).Result;
    }
}