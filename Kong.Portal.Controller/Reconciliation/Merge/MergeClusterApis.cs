using Kong.Portal.Controller.Model;
using Kong.Portal.Controller.Model.Extensions;
using Kong.Portal.Controller.Model.Repositories;

namespace Kong.Portal.Controller.Reconciliation.Merge;

public interface IMergeClusterApis
{
    void MergeNamespace(string nameSpace, IList<KongApi> apis);
}
    
public class MergeClusterApis : IMergeClusterApis
{
    private readonly IApiSwaggerRepository _apiSwaggerRepository;
    private readonly IKongApiDataRepository _kongApiDataRepository;
    private readonly IApiPodRepository _apiPodRepository;
    private readonly IKongRepository _kongRepository;
    private readonly ILogger<MergeClusterApis> _logger;
    private readonly IMergeOpenApiSchemas _mergeOpenApiSchemas;

    public MergeClusterApis(IApiSwaggerRepository apiSwaggerRepository,
        IKongApiDataRepository kongApiDataRepository,
        IApiPodRepository apiPodRepository,
        IKongRepository kongRepository,
        ILogger<MergeClusterApis> logger,
        IMergeOpenApiSchemas mergeOpenApiSchemas)
    {
        _apiSwaggerRepository = apiSwaggerRepository;
        _kongApiDataRepository = kongApiDataRepository;
        _apiPodRepository = apiPodRepository;
        _kongRepository = kongRepository;
        _logger = logger;
        _mergeOpenApiSchemas = mergeOpenApiSchemas;
    }
        
    public void MergeNamespace(string nameSpace, IList<KongApi> apis)
    {
        try
        {
            ProcessServices(nameSpace, apis);
        }
        catch (Exception e)
        {
            e.WithContext("namespace", nameSpace);
            
            _logger.LogError(e, "Api merge failed");
        }
    }

    private void ProcessServices(string nameSpace, IList<KongApi> apis)
    {
        var pods = _apiPodRepository.GetAll(nameSpace);

        var data = _kongApiDataRepository.GetAll(nameSpace);
        
        _logger.LogInformation("Checking if any apis have been updated.");
        
        var newApis = apis.Where(x => data.All(e => e.Name != x.Name)).ToList();

        var needsUpdate = data.Where(d => pods.Any(p => d.Name == p.Name && d.Updated < p.LastUpdated)).ToList();

        var updates = apis.Where(x => needsUpdate.Any(n => n.Name == x.Name)).ToList();
        
        updates.AddRange(newApis);
        
        var updated = new List<KongApi>();
        if (updates.Any())
        {
            _logger.LogInformation($"Updating {updates.Count} apis in cluster");
            
            updates.ForEach(x =>
            {
                var apiUpdated = UpdateSwaggerInCluster(x);
                if (apiUpdated)
                {
                    updated.Add(x);
                }
            });
        }

        _logger.LogInformation("Determining if any apis should be deleted");

        var deletes = data.Where(e => pods.All(p => e.Name != p.Name) && e.Name != "api").ToList();

        if (deletes.Any())
        {
            _logger.LogInformation($"Deleting {deletes.Count} apis, from cluster that are no longer deployed");
            deletes.ForEach(DeleteApi);
        }

        if (deletes.Any() || updated.Any())
        {
            _logger.LogInformation($"Found {updated.Count} updated apis and {deletes.Count} deleted apis, updating merged schema.");
            
            var freshData = _kongApiDataRepository.GetAll(nameSpace);
            freshData = freshData.Where(x => x.Name != "api" && x.Name != "namespace-state").ToList();

            var swagger = _mergeOpenApiSchemas.MergeNamespace(freshData, nameSpace);

            var mergeApi = new KongApiData
            {
                Name = "api",
                NameSpace = nameSpace,
                Updated = DateTime.Now,
                Data = swagger.ToBrotliBase64()
            };
            
            _logger.LogInformation("Merge done! persisting new schema.");
            
            _kongApiDataRepository.Delete(mergeApi);
            _kongApiDataRepository.Insert(mergeApi);
            
            _logger.LogInformation("Notifying kong portal of update.");
            
            _kongRepository.Delete(mergeApi.Name, mergeApi.NameSpace);
            _kongRepository.Update(mergeApi.Name, mergeApi.NameSpace, swagger);
        }
    }

    private void DeleteApi(KongApiData api)
    {
        _kongApiDataRepository.Delete(api);
    }

    private bool UpdateSwaggerInCluster(KongApi api)
    {
        try
        {
            _logger.LogInformation($"Updating specification for api {api.Name} in cluster.");
            
            var swagger = _apiSwaggerRepository.GetSwaggerJson(api);
            if (string.IsNullOrEmpty(swagger)) return false;

            var data = new KongApiData
            {
                Data = swagger.ToBrotliBase64(),
                Name = api.Name,
                NameSpace = api.NameSpace,
                Updated = DateTime.Now
            };
            
            _kongApiDataRepository.Delete(data);
            _kongApiDataRepository.Insert(data);
            return true;
        }
        catch (Exception e)
        {
            e.WithContext("Name", api.Name)
                .WithContext("Namespace", api.NameSpace)
                .WithContext("Port", api.Port)
                .WithContext("SwaggerUrl", api.Swagger);
            
            _logger.LogError(e, "Exception updating swagger spec in cluster");
        }

        return false;
    }
}