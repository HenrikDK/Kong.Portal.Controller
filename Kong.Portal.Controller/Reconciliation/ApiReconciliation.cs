using Kong.Portal.Controller.Model;
using Kong.Portal.Controller.Model.Repositories;
using Kong.Portal.Controller.Reconciliation.Merge;
using Kong.Portal.Controller.Reconciliation.Update;

namespace Kong.Portal.Controller.Reconciliation;

public interface IApiReconciliation
{
    void ProcessClusterApis();
}
    
public class ApiReconciliation : IApiReconciliation
{
    private readonly ILogger<ApiReconciliation> _logger;
    private readonly IConfiguration _configuration;
    private readonly IKongApiConfigRepository _kongApiConfigRepository;
    private readonly IMergeClusterApis _mergeClusterApis;
    private readonly IUpdateClusterApis _updateClusterApis;
    private readonly IKongApiRepository _kongApiRepository;
    private Lazy<IList<string>> _nameSpaces;

    public ApiReconciliation(ILogger<ApiReconciliation> logger,
        IConfiguration configuration,
        IKongApiConfigRepository kongApiConfigRepository,
        IMergeClusterApis mergeClusterApis,
        IUpdateClusterApis updateClusterApis,
        IKongApiRepository kongApiRepository)
    {
        _logger = logger;
        _configuration = configuration;
        _kongApiConfigRepository = kongApiConfigRepository;
        _mergeClusterApis = mergeClusterApis;
        _updateClusterApis = updateClusterApis;
        _kongApiRepository = kongApiRepository;

        _nameSpaces = new Lazy<IList<string>>(() =>
        {
            var namespaces = _configuration.GetValue<string>("monitor-namespaces");
            if (string.IsNullOrEmpty(namespaces)) return new List<string>();
            return namespaces.Split(",").ToList();
        });
    }
       
    public void ProcessClusterApis()
    {
        try
        {
            _logger.LogInformation($"Updating..");

            ReconcileApis();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception processing cluster apis");
        }
    }

    private void ReconcileApis()
    {
        var apis = _kongApiRepository.GetAll();

        var apisInNamespace = apis.ToLookup(x => x.NameSpace);

        if (_nameSpaces.Value.Any())
        {
            _logger.LogInformation($"Monitoring namespaces: {string.Join(",", _nameSpaces.Value)}");
            var monitored = apisInNamespace.Where(x => _nameSpaces.Value.Contains(x.Key)).ToList();
            
            monitored.ForEach(x => ReconcileNamespace(x.Key, x.ToList()));
            return;
        }
        
        _logger.LogInformation("Monitoring all namespaces");
        
        apisInNamespace.ForEach(x => ReconcileNamespace(x.Key, x.ToList()));
    }

    private void ReconcileNamespace(string nameSpace, List<KongApi> apis)
    {
        if (_nameSpaces.Value.Any() && !_nameSpaces.Value.Contains(nameSpace)) return;
        
        _logger.LogInformation($"Found {apis.Count} apis in namespace {nameSpace}");

        var config = _kongApiConfigRepository.GetFirstIn(nameSpace);

        if (config != null)
        {
            _mergeClusterApis.MergeNamespace(nameSpace, apis);
            return;
        }
        
        _updateClusterApis.UpdateApis(nameSpace, apis);
    }
}