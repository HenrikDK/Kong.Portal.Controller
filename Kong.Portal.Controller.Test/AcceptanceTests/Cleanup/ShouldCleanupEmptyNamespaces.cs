using Kong.Portal.Controller.Model;
using Kong.Portal.Controller.Model.Extensions;
using Kong.Portal.Controller.Model.Repositories;
using Kong.Portal.Controller.Reconciliation;
using Kong.Portal.Controller.Reconciliation.Merge;
using Kong.Portal.Controller.Reconciliation.Update;

namespace Kong.Portal.Controller.Test.AcceptanceTests.Cleanup;

public class ShouldCleanupEmptyNamespaces : AcceptanceTest
{
    private KongApiData _data;
    private ApiEntry _entry;

    public ShouldCleanupEmptyNamespaces()
    {
        _registry.AddSingleton(Substitute.For<IUpdateClusterApis>());
        _registry.AddSingleton(Substitute.For<IMergeOpenApiSchemas>());

        _registry.AddSingleton(Substitute.For<IApiPodRepository>());
        _registry.AddSingleton(Substitute.For<IKongRepository>());
        _registry.AddSingleton(Substitute.For<IKongApiRepository>());
        _registry.AddSingleton(Substitute.For<IKongApiConfigRepository>());
    }
        
    public void GivenTheLastApiInNamespaceIsNoLongerDeployed()
    {
        _entry = new ApiEntry
        {
            LastUpdated = DateTime.Now.AddHours(-2),
            Name = "test-api"
        };
        
        _data = new KongApiData
        {
            Name = "namespace-state",
            NameSpace = "test-ns",
            Updated = DateTime.Now.AddHours(-2),
            Data = JsonConvert.SerializeObject(new List<ApiEntry>{_entry}).ToBrotliBase64()
        };
        
        var dataRepository = Substitute.For<IKongApiDataRepository>();
        dataRepository.GetAll().Returns(new List<KongApiData> {_data});
        _registry.AddSingleton(dataRepository);
    }
    
    public void WhenTheSystemIsRunning()
    {
        BuildContainer();
        var scheduler = _container.GetInstance<IReconciliationScheduler>();
        _tokenSource.CancelAfter(500);
        try
        {
            scheduler.RunOnInterval(_tokenSource.Token, TimeSpan.FromMinutes(2));
        }
        catch (Exception e) { }
    }

    public void ThenTheLastApiIsRemovedFromTheDeveloperPortal()
    {
        var kongRepository = _container.GetInstance<IKongRepository>();
        kongRepository.Received().Delete(_entry.Name, _data.NameSpace);
    }
        
    public void AndThenTheStateTrackingDataObjectIsDeleted()
    {
        var dataRepository = _container.GetInstance<IKongApiDataRepository>();
        dataRepository.Received().Delete(_data);
    }
}