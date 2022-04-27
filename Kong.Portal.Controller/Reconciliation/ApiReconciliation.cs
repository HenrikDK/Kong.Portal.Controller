using Kong.Portal.Controller.Model;
using Kong.Portal.Controller.Model.Repositories;
using Kong.Portal.Controller.Reconciliation.Merge;
using Kong.Portal.Controller.Reconciliation.Update;
using MoreLinq;

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

        _nameSpaces = new Lazy<IList<string>>(() => configuration.GetValue<string>("monitor-namespaces").Split(",").ToList());
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
            _logger.LogError(e, "Error processing cluster apis");
        }
    }

    private void ReconcileApis()
    {
        
        var apis = _kongApiRepository.GetAll();

        var apisInNamespace = apis.ToLookup(x => x.NameSpace);
        
        apisInNamespace.ForEach(x => ReconcileNamespace(x.Key, x.ToList()));
    }

    private void ReconcileNamespace(string nameSpace, List<KongApi> apis)
    {
        var monitored = _nameSpaces.Value;
        if (monitored.Any() && !monitored.Contains(nameSpace)) return;

        var config = _kongApiConfigRepository.GetFirstIn(nameSpace);

        if (config != null)
        {
            _mergeClusterApis.MergeNamespace(nameSpace, apis);
            return;
        }
        
        _updateClusterApis.UpdateApis(nameSpace, apis);
    }
}