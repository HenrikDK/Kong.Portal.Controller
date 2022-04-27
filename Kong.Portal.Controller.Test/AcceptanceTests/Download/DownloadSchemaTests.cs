namespace Kong.Portal.Controller.Test.AcceptanceTests.Download;

[Story(AsA = "User", 
    IWant = "To download Api schema", 
    SoThat = "I can merge them into a combined schema")]
public class DownloadSchemaTests : Reporting
{
    [Test]
    public void ShouldDownloadSchemaFromDeployedApi()
    {
        new ShouldDownloadSchemaFromNewApi().BDDfy<DownloadSchemaTests>();
    }
    
    [Test]
    public void ShouldNotDownloadSchemaIfApiIsNotUpdated()
    {
        new ShouldNotDownloadSchemaIfApiIsNotUpdated().BDDfy<DownloadSchemaTests>();
    }
    
    [Test]
    public void ShouldDownloadSchemaFromUpdatedApi()
    {
        new ShouldDownloadSchemaFromUpdatedApi().BDDfy<DownloadSchemaTests>();
    }
}