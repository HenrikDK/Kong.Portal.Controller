using Kong.Portal.Controller.Model;
using Kong.Portal.Controller.Model.Extensions;
using Kong.Portal.Controller.Model.Repositories;
using Newtonsoft.Json.Linq;

namespace Kong.Portal.Controller.Reconciliation.Update;

public interface IUpdateClusterApis
{
    void UpdateApis(string nameSpace, IList<KongApi> apis);
}
    
public class UpdateClusterApis : IUpdateClusterApis
{
    private readonly ILogger<UpdateClusterApis> _logger;
    private readonly IConfiguration _configuration;
    private readonly IApiPodRepository _apiPodRepository;
    private readonly IApiSwaggerRepository _apiSwaggerRepository;
    private readonly IKongRepository _kongRepository;
    private readonly IKongApiDataRepository _kongApiDataRepository;

    public UpdateClusterApis(ILogger<UpdateClusterApis> logger,
        IConfiguration configuration,
        IApiPodRepository apiPodRepository,
        IApiSwaggerRepository apiSwaggerRepository,
        IKongRepository kongRepository,
        IKongApiDataRepository kongApiDataRepository)
    {
        _logger = logger;
        _configuration = configuration;
        _apiPodRepository = apiPodRepository;
        _apiSwaggerRepository = apiSwaggerRepository;
        _kongRepository = kongRepository;
        _kongApiDataRepository = kongApiDataRepository;
    }

    public void UpdateApis(string nameSpace, IList<KongApi> apis)
    {
        try
        {
            _logger.LogInformation($"Updating {nameSpace} namespace in cluster");

            ProcessServices(nameSpace, apis);
            
            _logger.LogInformation("Namespace update complete.");
            
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Api updates failed");
        }
    }

    private void ProcessServices(string nameSpace, IList<KongApi> apis)
    {
        _logger.LogInformation("Loading data");

        var pods = _apiPodRepository.GetAll(nameSpace);

        var data = _kongApiDataRepository.GetAll(nameSpace);

        var state = data.FirstOrDefault(x => x.Name == "namespace-state");
        var entries = GetApiEntries(state);
        
        _logger.LogInformation("Determining apis to update");

        var newApis = apis.Where(x => entries.All(e => e.Name != x.Name)).ToList();

        var needsUpdate = entries.Where(d => pods.Any(p => d.Name == p.Name && d.LastUpdated < p.LastUpdated)).ToList();

        var updates = apis.Where(x => needsUpdate.Any(n => n.Name == x.Name)).ToList();
        
        updates.AddRange(newApis);

        var updated = new List<KongApi>();
        if (updates.Any())
        {
            _logger.LogInformation($"Updating {updates.Count} apis");
            
            updates.ForEach(x =>
            {
                var apiUpdated = UploadApiToKong(x);
                if (apiUpdated)
                {
                    updated.Add(x);
                }
            });
        }

        _logger.LogInformation("Determining apis to be deleted");

        var deletes = entries.Where(e => pods.All(p => e.Name != p.Name)).ToList();

        if (deletes.Any())
        {
            _logger.LogInformation($"Deleting {deletes.Count} apis, that are no longer deployed");
            deletes.ForEach(x => DeleteApi(x, nameSpace));
        }

        PersistNamespaceState(nameSpace, deletes, updated, entries, newApis);
    }

    private void PersistNamespaceState(string nameSpace, List<ApiEntry> deletes, List<KongApi> updated, IList<ApiEntry> entries, List<KongApi> newApis)
    {
        if (!deletes.Any() && !updated.Any()) return;

        var remaining = entries.Except(deletes).ToList();
        
        remaining.ForEach(x =>
        {
            if (updated.Any(u => u.Name == x.Name))
            {
                x.LastUpdated = DateTime.Now;
            }
        });

        var newEntries = newApis.Select(x => new ApiEntry {Name = x.Name, LastUpdated = DateTime.Now}).ToList();
        newEntries.AddRange(remaining);

        var data = JsonConvert.SerializeObject(newEntries);

        var state = new KongApiData
        {
            Name = "namespace-state",
            NameSpace = nameSpace,
            Data = data.ToBrotliBase64(),
            Updated = DateTime.Now
        };
        
        _kongApiDataRepository.Delete(state);
        _kongApiDataRepository.Insert(state);
    }

    private bool UploadApiToKong(KongApi api)
    {
        try
        {
            var swagger = _apiSwaggerRepository.GetSwaggerJson(api);
            if (string.IsNullOrEmpty(swagger)) return false;

            var json = JObject.Parse(swagger);
            var suffix = _configuration.GetValue<string>("ingress-suffix");

            var url = new JObject();
            url["url"] = $"https://{api.Name}.{api.NameSpace}.{suffix}";
            var array = new JArray();
            array.Add(url);
            json["servers"] = array;

            swagger = json.ToString();

            _kongRepository.Delete(api.Name, api.NameSpace);
            _kongRepository.Update(api.Name, api.NameSpace, swagger);
            return true;
        }
        catch (Exception e)
        {
            e.WithContext("Name", api.Name)
                .WithContext("Namespace", api.NameSpace)
                .WithContext("Port", api.Port)
                .WithContext("SwaggerUrl", api.Swagger);

            _logger.LogError(e, "Exception uploading api spec to kong");
        }

        return false;
    }

    private IList<ApiEntry> GetApiEntries(KongApiData state)
    {
        if (state == null) return new List<ApiEntry>();

        var json = state.Data.FromBrotliBase64();
        var result = JsonConvert.DeserializeObject<IList<ApiEntry>>(json);

        return result;
    }

    private void DeleteApi(ApiEntry entry, string nameSpace)
    {
        _kongRepository.Delete(entry.Name, nameSpace);
    }
}