using Kong.Portal.Ui.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .MinimumLevel.Information()
    .WriteTo.Console(new JsonFormatter())
    .CreateLogger();

FlurlHttp.ConfigureClient(K8sClient.Server, cli => cli.Settings.HttpClientFactory = new UntrustedHttpsClientFactory());
FlurlHttp.Configure(c =>
{
    c.JsonSerializer = new NewtonsoftJsonSerializer(new JsonSerializerSettings
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    });
});

builder.Host.UseLamar((context, registry) =>
{
    registry.Scan(x =>
    {
        x.AssemblyContainingType<Program>();
        x.WithDefaultConventions();
        x.LookForRegistries();
    });
});

builder.WebHost
    .ConfigureKestrel(x => x.ListenAnyIP(8080))
    .ConfigureLogging((context, config) =>
    {
        config.ClearProviders();
        config.AddSerilog();
    });

builder.Services.AddControllers();
builder.Services.AddMemoryCache();

var app = builder.Build();

if (args.Contains("debug") || Debugger.IsAttached || Environment.GetEnvironmentVariable("debug") != null )
{
    app.UseDeveloperExceptionPage();
}

app.UseSwaggerUI(x =>
{
    x.RoutePrefix = "";
    x.SwaggerEndpoint("/swagger.json", "Api");
    x.ConfigObject.AdditionalItems["tagsSorter"] = "alpha";
    x.ConfigObject.AdditionalItems["operationsSorter"] = "alpha";
    x.ConfigObject.DefaultModelsExpandDepth = -1;
    x.DocExpansion(DocExpansion.List);
});

app.UseRouting();
app.UseHttpMetrics();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapMetrics();
});

app.Run();

Log.CloseAndFlush();