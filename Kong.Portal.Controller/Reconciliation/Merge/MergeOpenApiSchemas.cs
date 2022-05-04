using System.Globalization;
using Kong.Portal.Controller.Model;
using Kong.Portal.Controller.Model.Extensions;
using Kong.Portal.Controller.Model.Repositories;
using MoreLinq.Extensions;
using Newtonsoft.Json.Linq;

namespace Kong.Portal.Controller.Reconciliation.Merge;

public interface IMergeOpenApiSchemas
{
    string MergeNamespace(IList<KongApiData> api, string nameSpace);
}

public class MergeOpenApiSchemas : IMergeOpenApiSchemas
{
    private string mainSpec = @"
{
  ""openapi"": ""3.0.1"",
  ""info"": {""title"": """", ""version"": ""v1""},
  ""paths"": {},
  ""components"": {""schemas"": {}},
  ""servers"": {""url"": """"}
}";
    private readonly IKongApiConfigRepository _kongApiConfigRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MergeOpenApiSchemas> _logger;
    private readonly IMergeOpenApiSchema _mergeOpenApiSchema;

    public MergeOpenApiSchemas(IKongApiConfigRepository kongApiConfigRepository,
        IConfiguration configuration,
        ILogger<MergeOpenApiSchemas> logger,
        IMergeOpenApiSchema mergeOpenApiSchema)
    {
        _kongApiConfigRepository = kongApiConfigRepository;
        _configuration = configuration;
        _logger = logger;
        _mergeOpenApiSchema = mergeOpenApiSchema;
    }
        
    public string MergeNamespace(IList<KongApiData> apis, string nameSpace)
    {
        var mainSchema = JObject.Parse(mainSpec);
        
        apis.ForEach(x => MergeServiceIntoMainSchema(mainSchema, x));

        UpdateSchemaMasterData(mainSchema, "api", nameSpace);

        return mainSchema.ToString();
    }
        
    private void MergeServiceIntoMainSchema(JObject mainSchema, KongApiData api)
    {
        try
        {
            _mergeOpenApiSchema.Execute(mainSchema, api);
        }
        catch (Exception e)
        {
            e.WithContext("Name", api.Name)
                .WithContext("Namespace", api.NameSpace);
            
            _logger.LogError(e, "Exception merging spec into main");
        }
    }
        
    private void UpdateSchemaMasterData(JObject mainSchema, string name, string nameSpace)
    {
        var suffix = _configuration.GetValue<string>("ingress-suffix");
        var url = new JObject();
        url["url"] = $"https://{name}.{nameSpace}.{suffix}";
        var array = new JArray();
        array.Add(url);
        mainSchema["servers"] = array;
        
        var configuration = _kongApiConfigRepository.GetFirstIn(nameSpace);
        if (configuration == null)
        {
            var defaultTitle = nameSpace.Replace("-", " ");
            defaultTitle = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(defaultTitle.ToLower()) + " API";
            mainSchema["info"]["title"] = defaultTitle;
            mainSchema["info"]["description"] = "Api metadata not configured";
            return;
        }

        mainSchema["info"]["title"] = configuration.Title;
        mainSchema["info"]["description"] = configuration.Description;

        if (!string.IsNullOrEmpty(configuration.TermsUrl))
        {
            mainSchema["info"]["termsOfService"] = configuration.TermsUrl;
        }
        if (!string.IsNullOrEmpty(configuration.ContactEmail))
        {
            mainSchema["info"]["contact"] = new JObject();
            mainSchema["info"]["contact"]["email"] = configuration.ContactEmail;
        }
            
        if (!string.IsNullOrEmpty(configuration.LicenseName))
        {
            mainSchema["info"]["license"] = new JObject();
            mainSchema["info"]["license"]["name"] = configuration.LicenseName;
            mainSchema["info"]["license"]["url"] = configuration.LicenseUrl;
        }

        if (configuration.SecurityScheme == "BasicAuth")
        {
            mainSchema["components"]["securitySchemes"] = new JObject();
            mainSchema["components"]["securitySchemes"]["basicAuth"] = new JObject();
            mainSchema["components"]["securitySchemes"]["basicAuth"]["type"] = "http";
            mainSchema["components"]["securitySchemes"]["basicAuth"]["scheme"] = "basic";
        }

        if (configuration.SecurityScheme == "ApiKey")
        {
            mainSchema["components"]["securitySchemes"] = new JObject();
            mainSchema["components"]["securitySchemes"]["ApiKeyAuth"] = new JObject();
            mainSchema["components"]["securitySchemes"]["ApiKeyAuth"]["type"] = "apiKey";
            mainSchema["components"]["securitySchemes"]["ApiKeyAuth"]["in"] = "header";
            mainSchema["components"]["securitySchemes"]["ApiKeyAuth"]["name"] = configuration.SecurityKeyName;
        }

        if (configuration.SecurityScheme == "JWTBearer")
        {
            mainSchema["components"]["securitySchemes"] = new JObject();
            mainSchema["components"]["securitySchemes"]["Bearer"] = new JObject();
            mainSchema["components"]["securitySchemes"]["Bearer"]["type"] = "http";
            mainSchema["components"]["securitySchemes"]["Bearer"]["description"] = "Please insert JWT into field";
            mainSchema["components"]["securitySchemes"]["Bearer"]["scheme"] = "bearer";
            mainSchema["components"]["securitySchemes"]["Bearer"]["bearerFormat"] = "JWT";
        }
    }
}