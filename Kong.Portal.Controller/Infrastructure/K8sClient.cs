namespace Kong.Portal.Controller.Infrastructure;

public interface IK8sClient
{
    public string AccessToken { get; }
    public string Host { get; }
    public bool InCluster { get; }
}

public class K8sClient : IK8sClient
{
    private const string _tokenPath = "/var/run/secrets/kubernetes.io/serviceaccount/token";
    
    private static Lazy<bool> _inCluster = new(() => IsInCluster());
    private static Lazy<(string host, string token)> _fileConfig = new(() => LoadFromConfig());
    private static Lazy<string> _clusterHost = new(() => GetClusterHost() );

    private readonly IMemoryCache _cache;

    public K8sClient(IMemoryCache cache)
    {
        _cache = cache;
        _fileConfig = new Lazy<(string host, string token)>(() => LoadFromConfig());
    }

    public string AccessToken
    {
        get
        {
            if (_inCluster.Value)
            {
                return _cache.GetOrCreate("cluster-token", x =>
                {
                    x.AbsoluteExpiration = DateTime.Now.AddMinutes(2);
                    return File.ReadAllText(_tokenPath);
                });
            }
            
            return _fileConfig.Value.token;
        }
    }

    public static string Server => _inCluster.Value ? _clusterHost.Value : _fileConfig.Value.host;
    
    public string Host => _inCluster.Value ? _clusterHost.Value : _fileConfig.Value.host;

    public bool InCluster => _inCluster.Value;
    
    private static bool IsInCluster()
    {
        var host = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST");
        var port = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_PORT");

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port))
        {
            return false;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return false;
        }

        return File.Exists(_tokenPath);
    }
    
    private static string GetClusterHost()
    {
        var host = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST");
        var port = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_PORT");
        return $"https://{host}:{port}";
    }

    private static (string host, string token) LoadFromConfig()
    {
        var location = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
            ? Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), @".kube\config") 
            : Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".kube/config");

        var file = File.ReadAllText(location);
        var yamlObject = new Deserializer().Deserialize(new StringReader(file));
        var json = JsonConvert.SerializeObject(yamlObject);
        dynamic config = JsonConvert.DeserializeObject<ExpandoObject>(json);
        var dict = (IDictionary<string, object>) config;
        var currentContext = "" + dict["current-context"];

        var server = "";
        foreach (var entry in config.clusters)
        {
            if (entry.name != currentContext) continue;
            server = entry.cluster.server;
            break;
        }

        var token = "";
        foreach (var entry in config.users)
        {
            if (entry.name != currentContext) continue;
            try
            {
                token = entry.user.token;
            } catch (Exception e) { }
            break;
        }

        return (server, token);
    }
}