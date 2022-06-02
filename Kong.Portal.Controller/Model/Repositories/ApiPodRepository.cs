using Kong.Portal.Controller.Infrastructure;

namespace Kong.Portal.Controller.Model.Repositories;

public interface IApiPodRepository
{
    IList<ApiPod> GetAll(string nameSpace);
}
    
public class ApiPodRepository : IApiPodRepository
{
    private readonly IK8sClient _client;
 
    public ApiPodRepository(IK8sClient client)
    {
        _client = client;
    }

    public IList<ApiPod> GetAll(string nameSpace)
    {
        var host = _client.Host;

        var pods = host.AppendPathSegment($"/api/v1/namespaces/{nameSpace}/pods")
            .WithOAuthBearerToken(_client.AccessToken)
            .GetJsonAsync().Result;

        var apiPods = new List<ApiPod>();
        foreach (var pod in pods.items)
        {
            apiPods.Add(new ApiPod
            {
                Name = pod.metadata.nam,
                NameSpace = (string)((IDictionary<string, object>)pod.metadata)["namespace"],
                LastUpdated = pod.metadata.creationTimestamp,
            });
        }

        var lookup = apiPods.ToLookup(x => new {x.Name, x.NameSpace});
        
        var result = lookup.Select(x => x.OrderBy(x => x.LastUpdated).First()).ToList();

        return result;
    }
}