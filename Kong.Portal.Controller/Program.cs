using Kong.Portal.Controller;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .MinimumLevel.Information()
    .WriteTo.Console(new JsonFormatter())
    .CreateLogger();

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