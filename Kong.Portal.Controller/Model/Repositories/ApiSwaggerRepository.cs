namespace Kong.Portal.Controller.Model.Repositories;

public interface IApiSwaggerRepository
{
    string GetSwaggerJson(KongApi api);
}
    
public class ApiSwaggerRepository : IApiSwaggerRepository
{
    private readonly IConfiguration _configuration;
    private Lazy<bool> _inCluster;

    public ApiSwaggerRepository(IConfiguration configuration)
    {
        _configuration = configuration;
        _inCluster = new Lazy<bool>(() => KubernetesClientConfiguration.IsInCluster());
    }

    public string GetSwaggerJson(KongApi api)
    {
        var suffix = _configuration.GetValue<string>("ingress-suffix");
        var host = _inCluster.Value
            ? $"http://{api.Name}.{api.NameSpace}:{api.Port}{api.Swagger}"
            : $"https://{api.Name.Substring(0, api.Name.Length - 4)}.{api.NameSpace}.{suffix}{api.Swagger}";
        
        var swagger = host
            .AllowHttpStatus("4xx")
            .GetStringAsync().Result;
        
        return swagger;
    }
}