namespace Kong.Portal.Controller.Test.AcceptanceTests;

public class Reporting
{
    [OneTimeSetUp]
    public void SetupReporting()
    {
        Configurator.BatchProcessors.HtmlReport.Disable();

        var report = new DefaultHtmlReportConfiguration();
        report.OutputFileName = typeof(ServiceHost).Namespace + ".AcceptanceTests.html";
        report.ReportHeader = typeof(ServiceHost).Namespace + " acceptance tests";
        report.ReportDescription = "Automated tests of business critical functionality";

        Configurator.BatchProcessors.Add(new HtmlReporter(report));
    }
}