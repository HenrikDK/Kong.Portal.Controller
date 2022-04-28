namespace Kong.Portal.Controller.Test.AcceptanceTests.Update;

[Story(AsA = "User", 
    IWant = "To update api schemas", 
    SoThat = "I don't have to update kong manually")]
public class UpdateServiceTests : Reporting
{
    [Test]
    public void ShouldUpdateKongWithNewApi()
    {
        new ShouldUpdateKongWithNewApi().BDDfy<UpdateServiceTests>();
    }
    
    [Test]
    public void ShouldUpdateKongWithUpdatedApi()
    {
        new ShouldUpdateKongWithUpdatedApi().BDDfy<UpdateServiceTests>();
    }

    [Test]
    public void ShouldDeleteApiNoLongerInCluster()
    {
        new ShouldDeleteApiNoLongerInCluster().BDDfy<UpdateServiceTests>();
    }

    [Test]
    public void ShouldAddIngressInfoToSpecWhenUpdatingKong()
    {
        new ShouldAddIngressInfoToSpecWhenUpdatingKong().BDDfy<UpdateServiceTests>();
    }

    [Test]
    public void ShouldLogErrorIfUpdateFails()
    {
        new ShouldLogErrorIfUpdateFails().BDDfy<UpdateServiceTests>();
    }

    [Test]
    public void ShouldPersistNamespaceStateInCluster()
    {
        new ShouldPersistNamespaceStateInCluster().BDDfy<UpdateServiceTests>();
    }
}