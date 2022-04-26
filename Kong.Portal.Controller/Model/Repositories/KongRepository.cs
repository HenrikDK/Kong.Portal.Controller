namespace Kong.Portal.Controller.Model.Repositories;

public interface IKongRepository
{
    void Update(string name, string nameSpace, string data);
    void Delete(string name, string nameSpace);
}
    
public class KongRepository : IKongRepository
{
    private readonly IConfiguration _config;

    public KongRepository(IConfiguration config)
    {
        _config = config;
    }

    public void Update(string name, string nameSpace, string data)
    {
        var host = _config.GetValue<string>("kong-admin-url");
        var token = _config.GetValue<string>("kong-token");

        var result = host.AppendPathSegment($"/k8s-{name}-{nameSpace}.json")
            .WithHeader("Kong-Admin-Token", token)
            .PostJsonAsync(new {data}).Result;
    }

    public void Delete(string name, string nameSpace)
    {
        var host = _config.GetValue<string>("kong-admin-url");
        var token = _config.GetValue<string>("kong-token");

        var result = host.AppendPathSegment($"/k8s-{name}-{nameSpace}.json")
            .WithHeader("Kong-Admin-Token", token)
            .DeleteAsync().Result;
    }
}