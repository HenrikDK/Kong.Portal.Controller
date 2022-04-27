namespace Kong.Portal.Controller.Test.AcceptanceTests.Update;

[Story(AsA = "User", 
    IWant = "To update api schemas", 
    SoThat = "I don't have to update kong manually")]
public class UpdateServiceTests : Reporting
{
    [Test]
    public void ASingleServiceShouldNotBeMerged()
    {
        new ASingleServiceShouldNotBeMerged().BDDfy<UpdateServiceTests>();
    }
}