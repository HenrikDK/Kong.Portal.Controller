using Kong.Portal.Controller.Model;
using Kong.Portal.Controller.Model.Repositories;
using Kong.Portal.Controller.Reconciliation;
using Kong.Portal.Controller.Reconciliation.Cleanup;
using Kong.Portal.Controller.Reconciliation.Merge;
using Kong.Portal.Controller.Reconciliation.Update;

namespace Kong.Portal.Controller.Test.AcceptanceTests.Merge;

public class ShouldNotMergeApisIfConfigurationIsMissingFromNamespace : AcceptanceTest
{
    private KongApi _api;

    public ShouldNotMergeApisIfConfigurationIsMissingFromNamespace()
    {
        _registry.AddSingleton(Substitute.For<ICleanupClusterApis>());
        _registry.AddSingleton(Substitute.For<IUpdateClusterApis>());
        _registry.AddSingleton(Substitute.For<IMergeClusterApis>());
        
        _registry.AddSingleton(Substitute.For<IApiPodRepository>());
        _registry.AddSingleton(Substitute.For<IKongRepository>());
        _registry.AddSingleton(Substitute.For<IKongApiDataRepository>());
    }
        
    public void GivenANewApiHasBeenDeployed()
    {
        _api = new KongApi
        {
            Name = "test-api",
            NameSpace = "test-ns",
            Port = 8080,
            Swagger = "/swagger.json"
        };

        var apiRepository = Substitute.For<IKongApiRepository>();
        apiRepository.GetAll().Returns(new List<KongApi> {_api});
        _registry.AddSingleton(apiRepository);
    }

    public void AndGivenTheNamespaceIsMissingConfigurationForMergeApi()
    {
        _registry.AddSingleton(Substitute.For<IKongApiConfigRepository>());
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

    public void ThenTheApiIsMergedIntoACohesiveWhole()
    {
        var merge = _container.GetInstance<IMergeClusterApis>();
        merge.DidNotReceive().MergeNamespace(_api.NameSpace, Arg.Any<IList<KongApi>>());

        var update = _container.GetInstance<IUpdateClusterApis>();
        update.Received().UpdateApis(_api.NameSpace, Arg.Any<IList<KongApi>>());
    }
}