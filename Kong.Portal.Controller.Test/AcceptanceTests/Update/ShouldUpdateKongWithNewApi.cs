using Kong.Portal.Controller.Model;
using Kong.Portal.Controller.Model.Repositories;
using Kong.Portal.Controller.Reconciliation;
using Kong.Portal.Controller.Reconciliation.Cleanup;
using Kong.Portal.Controller.Reconciliation.Merge;

namespace Kong.Portal.Controller.Test.AcceptanceTests.Update;

public class ShouldUpdateKongWithNewApi : AcceptanceTest
{
    private KongApi _api;
    private string _json;
    private IApiSwaggerRepository _swaggerRepository;

    public ShouldUpdateKongWithNewApi()
    {
        _registry.AddSingleton(Substitute.For<ICleanupClusterApis>());
        _registry.AddSingleton(Substitute.For<IMergeOpenApiSchema>());
        
        _registry.AddSingleton(Substitute.For<IApiPodRepository>());
        _registry.AddSingleton(Substitute.For<IKongRepository>());
        _registry.AddSingleton(Substitute.For<IKongApiDataRepository>());
        _registry.AddSingleton(Substitute.For<IKongApiConfigRepository>());
    }
        
    public void GivenANewApiHasBeenDeployed()
    {
        _api = new KongApi
        {
            Name = "test-api",
            Port = 8080,
            Swagger = "/swagger.json",
            NameSpace = "test-ns"
        };
        
        var apiRepository = Substitute.For<IKongApiRepository>();
        apiRepository.GetAll().Returns(new List<KongApi>{_api});
        _registry.AddSingleton(apiRepository);
    }

    public void AndGivenTheServiceIsRunning()
    {
        _json = @"{ ""test_json"":{} }";

        _swaggerRepository = Substitute.For<IApiSwaggerRepository>();
        _swaggerRepository.GetSwaggerJson(_api).Returns(_json);
        _registry.AddSingleton(_swaggerRepository);
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

    public void ThenTheKongDeveloperPortalIsUpdated()
    {
        var kongRepository = _container.GetInstance<IKongRepository>();
        kongRepository.Received().Update(_api.Name, _api.NameSpace, Arg.Any<string>());
    }
}