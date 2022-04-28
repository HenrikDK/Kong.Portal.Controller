using Kong.Portal.Controller.Model;
using Kong.Portal.Controller.Model.Extensions;
using Kong.Portal.Controller.Model.Repositories;

namespace Kong.Portal.Controller.Reconciliation.Cleanup;

public interface ICleanupClusterApis
{
    void CleanupCluster();
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

    public void CleanupCluster()
    {
        CleanupUnusedDataObjects();
        
        CleanupEmptyNamespaces();

        CleanupUnusedMergedApiDataObjects();
    }
    
    private void CleanupUnusedDataObjects()
    {
        try
        {
            var apis = _kongApiRepository.GetAll();

            var data = _kongApiDataRepository.GetAll();

            var notMatched = data.Where(d => !apis.Any(a => a.NameSpace == d.NameSpace && a.Name == d.Name)).ToList();

            notMatched = notMatched.Where(x => x.Name != "api" && x.Name != "namespace-state").ToList();
            
            notMatched.ForEach(CleanupClusterApi);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception during Api cleanup");
        }
    }

    private void CleanupClusterApi(KongApiData api)
    {
        _kongApiDataRepository.Delete(api);
        
        _kongRepository.Delete(api.Name, api.NameSpace);
    }
    
    private void CleanupEmptyNamespaces()
    {
        try
        {
            var data = _kongApiDataRepository.GetAll();

            var state = data.Where(x => x.Name == "namespace-state").ToList();
            
            state.ForEach(CleanupEmptyNamespace);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception during Api cleanup");
        }
    }

    private void CleanupEmptyNamespace(KongApiData data)
    {
        var apis = _kongApiRepository.GetAll(data.NameSpace);

        var entries = GetApiEntries(data);

        var ghosts = entries.Where(y => !apis.Any(a => a.Name == y.Name && a.NameSpace == data.NameSpace)).ToList();

        var remaining = entries.Except(ghosts);
        
        if (remaining.Any()) return; // normal reconciliation will handle these
        
        ghosts.ForEach(x => _kongRepository.Delete(x.Name, data.NameSpace));
        
        _kongApiDataRepository.Delete(data);
    }

    private List<ApiEntry> GetApiEntries(KongApiData state)
    {
        if (state != null)
        {
            var json = state.Data.FromBrotliBase64();
            var result = JsonConvert.DeserializeObject<List<ApiEntry>>(json);
            return result;
        }

        return new List<ApiEntry>();
    }
    
    private void CleanupUnusedMergedApiDataObjects()
    {
        try
        {
            var data = _kongApiDataRepository.GetAll();

            var state = data.Where(x => x.Name == "api").ToList();
            
            state.ForEach(CleanupEmptyMergedApi);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Cleanup of cluster data failed.");
        }
    }

    private void CleanupEmptyMergedApi(KongApiData data)
    {
        var apis = _kongApiRepository.GetAll(data.NameSpace);
        
        if (apis.Any()) return;
        
        _kongApiDataRepository.Delete(data);
        
        _kongRepository.Delete(data.Name, data.NameSpace);
    }
}