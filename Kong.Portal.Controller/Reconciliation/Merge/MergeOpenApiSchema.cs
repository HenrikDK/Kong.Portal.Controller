using Kong.Portal.Controller.Model;
using Kong.Portal.Controller.Model.Extensions;

namespace Kong.Portal.Controller.Reconciliation.Merge;

public interface IMergeOpenApiSchema
{
    void Execute(JObject mainSchema, KongApiData api);
}
    
public class MergeOpenApiSchema : IMergeOpenApiSchema
{
    public void Execute(JObject mainSchema, KongApiData api)
    {
        var schema = api.Data.FromBrotliBase64(); 
        var json = JObject.Parse(schema);

        var types = json["components"]["schemas"]?.ToKeyValuePairs() ?? new List<(string Key, object Value)>();
        foreach (var type in types)
        {
            schema = schema.Replace($"#/components/schemas/{type.Key}", $"#/components/schemas/{api.Name}_{type.Key}");
        }

        var serviceJson = JObject.Parse(schema);
        foreach (var type in types)
        {
            mainSchema["components"]["schemas"][$"{api.Name}_{type.Key}"] = serviceJson["components"]["schemas"][type.Key];
        }

        var paths = json["paths"].ToKeyValuePairs();
        foreach (var path in paths)
        {
            mainSchema["paths"][path.Key] = serviceJson["paths"][path.Key];
        }
    }
}