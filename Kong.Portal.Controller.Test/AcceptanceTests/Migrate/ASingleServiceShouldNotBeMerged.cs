namespace Kong.Portal.Controller.Test.AcceptanceTests.Migrate;

public class ASingleServiceShouldNotBeMerged : AcceptanceTest
{/*
    private ServiceDefinition _service;

    public ASingleServiceShouldNotBeMerged()
    {
        _registry.AddSingleton(Substitute.For<IInsertMergedSchema>());
        _registry.AddSingleton(Substitute.For<IUpdateServiceStatus>());
        _registry.AddSingleton(Substitute.For<IMergeOpenApiSchema>());

        var getConfiguration = Substitute.For<IGetConfigurationCached>();
        var configuration = new Model.Configuration{UrlFilter = "/v1/", JsonEndpoint = "/swagger.json"};
        getConfiguration.Execute().Returns(configuration);
        _registry.AddSingleton(getConfiguration);
    }
        
    public void GivenASingleServiceHasBeenDeployed()
    {
        var getDeploymentCount = Substitute.For<IGetDeploymentCount>();
        getDeploymentCount.Execute().Returns(1);
        _registry.AddSingleton(getDeploymentCount);
            
        _service = new ServiceDefinition
        {
            Id = 1,
            Status = ServiceStatus.Fetched,
            JsonData = "{'paths':{}}"
        };
        var getActiveServices = Substitute.For<IGetActiveServices>();
        getActiveServices.Execute().Returns(new List<ServiceDefinition> {_service});
        _registry.AddSingleton(getActiveServices);
    }

    public void WhenTheSystemIsRunning()
    {
        _container = new Container(_registry);
        var host = _container.GetInstance<ServiceHost>();
        host.StartAsync(_tokenSource.Token).Wait();
        Thread.Sleep(500);
        _tokenSource.Cancel();
        host.StopAsync(_tokenSource.Token);
    }

    public void ThenNoSchemaIsMerged()
    {
        var merge = _container.GetInstance<IMergeOpenApiSchema>();
        merge.DidNotReceive().Execute(Arg.Any<JObject>(), Arg.Any<ServiceDefinition>());
    }
        
    public void AndThenANewSchemaIsSaved()
    {
        var insert = _container.GetInstance<IInsertMergedSchema>();
        insert.Received().Execute(Arg.Any<string>(), Arg.Any<int>());
    }

    public void AndThenTheServiceDefinitionIsUpdated()
    {
        var update = _container.GetInstance<IUpdateServiceStatus>();
        update.Received().Execute(Arg.Any<IList<ServiceDefinition>>());

        _service.Status.Should().Be(ServiceStatus.Done);
    }*/
}