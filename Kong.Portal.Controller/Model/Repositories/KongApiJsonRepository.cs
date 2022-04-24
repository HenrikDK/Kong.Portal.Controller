namespace Kong.Portal.Controller.Model.Repositories;

public interface IKongApiJsonRepository
{
    IList<KongApiJson> GetAll(string nameSpace);
    void Update(KongApiJson json);
}

public class KongApiJsonRepository : IKongApiJsonRepository
{
    private Lazy<KubernetesClientConfiguration> _configuration;

    public KongApiJsonRepository()
    {
        _configuration = new Lazy<KubernetesClientConfiguration>(() => KubernetesClientConfiguration.IsInCluster()
            ? KubernetesClientConfiguration.InClusterConfig()
            : KubernetesClientConfiguration.BuildDefaultConfig());
    }
    
    public IList<KongApiJson> GetAll(string nameSpace)
    {
        throw new NotImplementedException();
    }

    public void Update(KongApiJson json)
    {
        throw new NotImplementedException();
    }
}