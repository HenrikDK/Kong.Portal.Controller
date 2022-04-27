namespace Kong.Portal.Controller.Test.AcceptanceTests.Merge;

[Story(AsA = "User", 
    IWant = "To merge Api schemas", 
    SoThat = "I can present a combined cohesive api")]
public class MergeSchemaTests : Reporting
{
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
    }
}