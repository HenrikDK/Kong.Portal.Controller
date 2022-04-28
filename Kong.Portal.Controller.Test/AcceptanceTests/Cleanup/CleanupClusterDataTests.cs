namespace Kong.Portal.Controller.Test.AcceptanceTests.Cleanup;

[Story(AsA = "User", 
    IWant = "To cleanup cluster objects", 
    SoThat = "I avoid excessive cluster usage")]
public class CleanupClusterDataTests : Reporting
{
    [Test]
    public void ShouldCleanupUnusedClusterObjects()
    {
        new ShouldCleanupUnusedClusterObjects().BDDfy<CleanupClusterDataTests>();
    }
    
    [Test]
    public void ShouldCleanupEmptyNamespaces()
    {
        new ShouldCleanupEmptyNamespaces().BDDfy<CleanupClusterDataTests>();
    }
    
    [Test]
    public void ShouldCleanupUnusedMergeApiDataObjects()
    {
        new ShouldCleanupUnusedMergeApiDataObjects().BDDfy<CleanupClusterDataTests>();
    }
}