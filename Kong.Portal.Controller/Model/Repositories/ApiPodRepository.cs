namespace Kong.Portal.Controller.Model.Repositories;

public interface IApiPodRepository
{
    IList<ApiPod> GetAll(string nameSpace);
}
    
public class ApiPodRepository : IApiPodRepository
{
    private Lazy<KubernetesClientConfiguration> _config;

    public ApiPodRepository()
    {
        _config = new Lazy<KubernetesClientConfiguration>(() => KubernetesClientConfiguration.IsInCluster()
            ? KubernetesClientConfiguration.InClusterConfig()
            : KubernetesClientConfiguration.BuildDefaultConfig());
    }

    public IList<ApiPod> GetAll(string nameSpace)
    {
        var host = _config.Value.Host;

        var pods = host.AppendPathSegment($"/api/v1/namespaces/{nameSpace}/pods")
            .WithOAuthBearerToken(_config.Value.AccessToken)
            .GetJsonAsync().Result;

        var apiPods = new List<ApiPod>();
        foreach (var pod in pods.items)
        {
            var name = pod.metadata.name.ToString();
            if (!name.Contains("-api")) continue;

            if (name.Contains("api-"))
                name = name.Substring(0, name.IndexOf("-api")+4);
            
            apiPods.Add(new ApiPod
            {
                Name = name,
                NameSpace = (string)((IDictionary<string, object>)pod.metadata)["namespace"],
                LastUpdated = pod.metadata.creationTimestamp,
            });
        }

        var lookup = apiPods.ToLookup(x => new {x.Name, x.NameSpace});
        
        var result = lookup.Select(x => x.OrderByDescending(x => x.LastUpdated).First()).ToList();

        return result;
    }
}