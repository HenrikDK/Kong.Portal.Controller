using Kong.Portal.Controller.Model;
using Kong.Portal.Controller.Model.Repositories;

namespace Kong.Portal.Controller.Reconciliation.Cleanup;

public interface ICleanupClusterApis
{
    void CleanupApiData();
}
    
public class CleanupClusterApis : ICleanupClusterApis
{
    private readonly ILogger<CleanupClusterApis> _logger;
    private readonly IKongRepository _kongRepository;
    private readonly IKongApiRepository _kongApiRepository;
    private readonly IKongApiDataRepository _kongApiDataRepository;

    public CleanupClusterApis(ILogger<CleanupClusterApis> logger,
        IKongRepository kongRepository,
        IKongApiRepository kongApiRepository,
        IKongApiDataRepository kongApiDataRepository)
    {
        _logger = logger;
        _kongRepository = kongRepository;
        _kongApiRepository = kongApiRepository;
        _kongApiDataRepository = kongApiDataRepository;
    }

    public void CleanupApiData()
    {
        try
        {
            ProcessClusterApis();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception during Api cleanup");
        }
    }

    private void ProcessClusterApis()
    {
        var apis = _kongApiRepository.GetAll();

        var data = _kongApiDataRepository.GetAll();

        var notMatched = data.Where(d => !apis.Any(a => a.NameSpace == d.NameSpace && a.Name == d.Name)).ToList();

        notMatched = notMatched.Where(x => x.Name != "api" && x.Name != "namespace-state").ToList();
        
        notMatched.ForEach(CleanupClusterApi);
    }

    private void CleanupClusterApi(KongApiData api)
    {
        _kongApiDataRepository.Delete(api);
        
        _kongRepository.Delete(api.Name, api.NameSpace);
    }
}