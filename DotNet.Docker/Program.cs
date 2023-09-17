using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Exceptions;

Serilog.Debugging.SelfLog.Enable(Console.Error);

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
    .AddEnvironmentVariables()
    .Build();

using var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.WithExceptionDetails()
    .CreateLogger();

string serverUrl = configuration.GetSection("Serilog:WriteTo:1:Args:serverUrl").Value!;
logger.Information(serverUrl);

CancellationTokenSource cts = new();

AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);

void CurrentDomain_ProcessExit(object? s, EventArgs e)
{
    logger.Information("Canceling...");
    cts.Cancel();
};

int counter = 0;
int max = args.Length is not 0 ? Convert.ToInt32(args[0]) : int.MaxValue;
while (counter < max)
{
    logger.Information($"Counter: {++counter}");
    await Task.Delay(TimeSpan.FromMilliseconds(1_000), cts.Token);
}