using Kong.Portal.Controller.Infrastructure;

namespace Kong.Portal.Controller.Model.Repositories;

public interface IApiSwaggerRepository
{
    string GetSwaggerJson(KongApi api);
}
    
public class ApiSwaggerRepository : IApiSwaggerRepository
{
    private readonly IConfiguration _configuration;
    private readonly IK8sClient _client;

    public ApiSwaggerRepository(IConfiguration configuration, IK8sClient client)
    {
        _configuration = configuration;
        _client = client;
    }

    public string GetSwaggerJson(KongApi api)
    {
        var suffix = _configuration.GetValue<string>("ingress-suffix");
        var host = _client.InCluster ? 
            $"http://{api.Name}.{api.NameSpace}:{api.Port}{api.Swagger}" :
            $"https://{api.Name.Substring(0, api.Name.Length - 4)}.{api.NameSpace}.{suffix}{api.Swagger}";
        
        var swagger = host
            .AllowHttpStatus("4xx")
            .GetStringAsync().Result;
        
        return swagger;
    }
}