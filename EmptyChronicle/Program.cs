using EmptyChronicle;
using EmptyChronicle.Hosting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables("PN_")
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(config)
    .CreateLogger();

var headlessConfig = new Configuration();
config.Bind(headlessConfig);

builder.Services
    .AddLibplanetServices(headlessConfig)
    .AddControllers();

var app = builder.Build();

app.UseCors(policyBuilder => policyBuilder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.MapControllers();

app.MapGet("/", () => "Sample");

app.Run();