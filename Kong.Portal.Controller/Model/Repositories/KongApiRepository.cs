namespace Kong.Portal.Controller.Model.Repositories;

public interface IKongApiRepository
{
    IList<KongApi> GetAll();
    IList<KongApi> GetAll(string nameSpace);
}
    
public class KongApiRepository : IKongApiRepository
{
    private Lazy<KubernetesClientConfiguration> _config;

    public KongApiRepository()
    {
        _config = new Lazy<KubernetesClientConfiguration>(() => KubernetesClientConfiguration.IsInCluster()
            ? KubernetesClientConfiguration.InClusterConfig()
            : KubernetesClientConfiguration.BuildDefaultConfig());
    }
    
    public IList<KongApi> GetAll()
    {
        var host = _config.Value.Host;

        var pods = host.AppendPathSegment("/apis/henrik.dk/v1/kong-apis")
            .WithOAuthBearerToken(_config.Value.AccessToken)
            .GetJsonAsync().Result;

        var apis = new List<KongApi>();
        foreach (var api in pods.items)
        {
            apis.Add(new KongApi
            {
                Name = api.metadata.name,
                NameSpace = (string)((IDictionary<string, object>)api.metadata)["namespace"],
                Port = (int) api.spec.port,
                Swagger = api.spec.swagger
            });
        }

        return apis;
    }
    
    public IList<KongApi> GetAll(string nameSpace)
    {
        var host = _config.Value.Host;

        var pods = host.AppendPathSegment($"/apis/henrik.dk/v1/namespaces/{nameSpace}/kong-apis")
            .WithOAuthBearerToken(_config.Value.AccessToken)
            .GetJsonAsync().Result;

        var apis = new List<KongApi>();
        foreach (var api in pods.items)
        {
            apis.Add(new KongApi
            {
                Name = api.metadata.name,
                NameSpace = (string)((IDictionary<string, object>)api.metadata)["namespace"],
                Port = (int) api.spec.port,
                Swagger = api.spec.swagger
            });
        }

        return apis;
    }
}