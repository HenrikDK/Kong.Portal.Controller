namespace Kong.Portal.Controller.Model.Repositories;

public interface IKongApiRepository
{
    IList<KongApi> GetAll();
}
    
public class KongApiRepository : IKongApiRepository
{
    private Lazy<KubernetesClientConfiguration> _configuration;

    public KongApiRepository()
    {
        _configuration = new Lazy<KubernetesClientConfiguration>(() => KubernetesClientConfiguration.IsInCluster()
            ? KubernetesClientConfiguration.InClusterConfig()
            : KubernetesClientConfiguration.BuildDefaultConfig());
    }
    
    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public string GetFileContents(string path)
    {
        return File.ReadAllText(path);
    }

    public IList<KongApi> GetAll()
    {
        throw new NotImplementedException();
    }
}