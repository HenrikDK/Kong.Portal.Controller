namespace Kong.Portal.Controller.Model;

public class KongApiConfig
{
    public string Name { get; set; }
    public string NameSpace { get; set; }

    public string Title { get; set; }
    public string Description { get; set; }
    public string TermsUrl { get; set; }
    public string ContactEmail { get; set; }
    public string LicenseName { get; set; }
    public string LicenseUrl { get; set; }
    public string SecurityScheme { get; set; }
    public string SecurityKeyName { get; set; }
}