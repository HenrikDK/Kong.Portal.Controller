namespace Kong.Portal.Controller.Test.AcceptanceTests.Cleanup;

[Story(AsA = "User", 
    IWant = "To conserve cluster resources", 
    SoThat = "I avoid excessive maintenance costs in production")]
public class CleanupClusterDataTests : Reporting
{
    /*
    [Test]
    public void ASingleServiceShouldBeMerged()
    {
        new ASingleServiceShouldBeMerged().BDDfy<MergeSchemaTests>();
    }

    [Test]
    public void MultipleServiceSchemasShouldBeMerged()
    {
        new MultipleServiceSchemasShouldBeMerged().BDDfy<MergeSchemaTests>();
    }
        
    [Test]
    public void MergedServiceSchemasShouldHaveTheirTypesRenamed()
    {
        new MergedServiceSchemasShouldHaveTheirTypesRenamed().BDDfy<MergeSchemaTests>();
    }
    
    [Test]
    public void SecurityRequirementsShouldBeAddedAfterMerge()
    {
        new SecurityRequirementsShouldBeAddedAfterMerge().BDDfy<MergeSchemaTests>();
    }
        
    [Test]
    public void ApiShouldBeRenamedAfterMerge()
    {
        new ApiShouldBeRenamedAfterMerge().BDDfy<MergeSchemaTests>();
    }*/
}