namespace Kong.Portal.Controller.Test.AcceptanceTests.Merge;

[Story(AsA = "User", 
    IWant = "To merge Api schemas", 
    SoThat = "I can present a combined cohesive api")]
public class MergeSchemaTests : Reporting
{
    [Test]
    public void ShouldMergeApisWhenConfigurationIsPresentInNamespace()
    {
        new ShouldMergeApisWhenConfigurationIsPresentInNamespace().BDDfy<MergeSchemaTests>();
    }
    
    [Test]
    public void ShouldNotMergeApisIfConfigurationIsMissingFromNamespace()
    {
        new ShouldNotMergeApisIfConfigurationIsMissingFromNamespace().BDDfy<MergeSchemaTests>();
    }

    [Test]
    public void ShouldPersistNewInputApiSpecInCluster()
    {
        new ShouldPersistNewInputApiSpecInCluster().BDDfy<MergeSchemaTests>();
    }

    [Test]
    public void ShouldPersistUpdatedInputApiSpecInCluster()
    {
        new ShouldPersistUpdatedInputApiSpecInCluster().BDDfy<MergeSchemaTests>();
    }

    [Test]
    public void ShouldDeletePersistedSpecIfApiIsNoLongerInCluster()
    {
        //new ShouldDeletePersistedSpecIfApiIsNoLongerInCluster().BDDfy<MergeSchemaTests>();
    }

    [Test]
    public void ShouldPersistMergedApiSpecInCluster()
    {
        //new ShouldPersistMergedApiSpecInCluster().BDDfy<MergeSchemaTests>();
    }

    [Test]
    public void ShouldUpdateKongWithMergedApiSpec()
    {
        //new ShouldUpdateKongWithMergedApiSpec().BDDfy<MergeSchemaTests>();
    }

    [Test]
    public void ShouldMergeMultipleApisIntoOneMergedApi()
    {
        //new ShouldMergeMultipleApisIntoOneMergedApi().BDDfy<MergeSchemaTests>();
    }

    [Test]
    public void ShouldSetValuesInMergedSpecBasedOnConfiguration()
    {
        //new ShouldSetValuesInMergedSpecBasedOnConfiguration().BDDfy<MergeSchemaTests>();
    }

    [Test]
    public void ShouldRenameApiEntitiesDuringMergeToAvoidNamingCollisions()
    {
        //new ShouldRenameApiEntitiesDuringMergeToAvoidNamingCollisions().BDDfy<MergeSchemaTests>();
    }
}