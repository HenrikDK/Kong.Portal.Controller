namespace Kong.Portal.Controller.Test.AcceptanceTests.Migrate;

[Story(AsA = "User", 
    IWant = "To migrate database schemas", 
    SoThat = "I can maintain multiple database environments easily")]
public class MigrateDbTests : Reporting
{
    [Test]
    public void ASingleServiceShouldNotBeMerged()
    {
        new ASingleServiceShouldNotBeMerged().BDDfy<MigrateDbTests>();
    }
}