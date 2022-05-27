using Kong.Portal.Controller.Infrastructure;

namespace Kong.Portal.Controller.Model.Repositories;

public interface IKongApiConfigRepository
{
    KongApiConfig GetFirstIn(string nameSpace);
}
    
public class KongApiConfigRepository : IKongApiConfigRepository
{
    private readonly IK8sClient _client;

    public KongApiConfigRepository(IK8sClient client)
    {
        _client = client;
    }
    
    public KongApiConfig GetFirstIn(string nameSpace)
    {
        var host = _client.Host;

        var pods = host.AppendPathSegment($"/apis/henrik.dk/v1/namespaces/{nameSpace}/kong-api-configs")
            .WithOAuthBearerToken(_client.AccessToken)
            .GetJsonAsync().Result;

        var apis = new List<KongApiConfig>();
        foreach (var api in pods.items)
        {
            apis.Add(new KongApiConfig
            {
                Name = api.metadata.name,
                NameSpace = (string)((IDictionary<string, object>)api.metadata)["namespace"],
                Title = api.spec.title,
                Description = api.spec.description,
                ContactEmail = api.spec.contactEmail,
                LicenseName = api.spec.licenseName,
                LicenseUrl = api.spec.licenseUrl,
                SecurityScheme = api.spec.securityScheme,
                SecurityKeyName = api.spec.securityKeyName,
                TermsUrl = api.spec.termsUrl
            });
        }

        return apis.FirstOrDefault();
    }
}