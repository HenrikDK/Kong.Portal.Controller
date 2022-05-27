using Kong.Portal.Controller.Infrastructure;

namespace Kong.Portal.Controller.Model.Repositories;

public interface IKongApiRepository
{
    IList<KongApi> GetAll();
    IList<KongApi> GetAll(string nameSpace);
}
    
public class KongApiRepository : IKongApiRepository
{
    private readonly IK8sClient _client;

    public KongApiRepository(IK8sClient client)
    {
        _client = client;
    }
    
    public IList<KongApi> GetAll()
    {
        var host = _client.Host;

        var pods = host.AppendPathSegment("/apis/henrik.dk/v1/kong-apis")
            .WithOAuthBearerToken(_client.AccessToken)
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
        var host = _client.Host;

        var pods = host.AppendPathSegment($"/apis/henrik.dk/v1/namespaces/{nameSpace}/kong-apis")
            .WithOAuthBearerToken(_client.AccessToken)
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