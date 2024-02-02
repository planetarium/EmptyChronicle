using Cocona;
using EmptyChronicle;
using EmptyChronicle.Application;
using EmptyChronicle.Domain;
using EmptyChronicle.Hosting;
using Serilog;

CoconaApp.Run((
    [Option(Description = "Sync for heimdall mainnet.")]
    bool heimdall,
    [Option(Description = "Sync for odin mainnet.")]
    bool odin
    ) =>
{
    var configurationFileName = (heimdall, odin) switch
    {
        (true, true) => throw new CommandExitedException("You must select only one of the --heimdall and --odin options.", -1),
        (true, false) => "appsettings.heimdall.json",
        (false, true) => "appsettings.odin.json",
        (false, false) => throw new CommandExitedException("You must choose between the --heimdall option and the --odin option.", -1)
    };

    var builder = WebApplication.CreateBuilder(args);

    var config = new ConfigurationBuilder()
        .AddJsonFile(configurationFileName)
        .AddEnvironmentVariables("PN_")
        .Build();

    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(config)
        .CreateLogger();

    var headlessConfig = new Configuration();
    config.Bind(headlessConfig);

    builder.Services
        .AddLibplanetServices(headlessConfig)
        .AddRepositories()
        .AddApplicationServices()
        .AddControllers();

    var app = builder.Build();

    app.UseCors(policyBuilder => policyBuilder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

    app.MapControllers();

    app.MapGet("/", () => "Sample");

    app.Run();
});
