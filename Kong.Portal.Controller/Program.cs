using Kong.Portal.Controller;
using Kong.Portal.Controller.Infrastructure;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .MinimumLevel.Information()
    .WriteTo.Console(new JsonFormatter())
    .CreateLogger();

var config = KubernetesClientConfiguration.IsInCluster()
    ? KubernetesClientConfiguration.InClusterConfig()
    : KubernetesClientConfiguration.BuildDefaultConfig();

FlurlHttp.ConfigureClient(config.Host, cli => cli.Settings.HttpClientFactory = new UntrustedHttpsClientFactory());
FlurlHttp.Configure(c =>
{
    c.JsonSerializer = new NewtonsoftJsonSerializer(new JsonSerializerSettings
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    });
});

var builder = Host.CreateDefaultBuilder()
    .UseLamar((context, registry) => registry.Scan(x =>
    {
        x.AssemblyContainingType<Program>();
        x.WithDefaultConventions();
        x.LookForRegistries();
    }))
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<ServiceHost>();
        services.AddMemoryCache();
    })
    .ConfigureLogging((context, config) =>
    {
        config.ClearProviders();
        config.AddSerilog();
    });
    
var app = builder.Build();

app.Run();

Log.CloseAndFlush();