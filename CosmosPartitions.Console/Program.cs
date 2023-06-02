using System.Reflection;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace CosmosPartitions.Console;
public class Program
{

    public static async Task Main(string[] args)
    {
        var config = BuildConfiguration();
        var serviceProvider = BuildServiceProvider(config);
        
        var log = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
        Log.Logger = log;

        var commandName = args[0];
        var command = Assembly.GetExecutingAssembly().CreateInstance("CosmosPartitions.Console.Commands."+commandName) as IRequest;

        if (command == null)
        {
            Log.Error("Failed to find command '{CommandName}'", commandName);
            return;
        }

        var mediator = serviceProvider.GetRequiredService<IMediator>();
        Log.Information("Running Command '{CommandName}'", commandName);
        await mediator.Send(command);
        

        // var data = new Data();
        // var events = data.GetEvents(250000);
        //
        // System.Console.WriteLine("writing...");
        // var writer = serviceProvider.GetRequiredService<CosmosWriter>();
        // await writer.WriteEvents(events, "id-key", "/id");
        //
        // System.Console.WriteLine("Records written");
    }

    private static ServiceProvider BuildServiceProvider(IConfigurationRoot config)
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton(new CosmosClient(
                config["CosmosConnectionString"],
                new CosmosClientOptions
                {
                    AllowBulkExecution = true,
                    ApplicationName = "CosmosPartitionsTest",
                    ConnectionMode = ConnectionMode.Direct,
                    EnableContentResponseOnWrite = false
                })
            )
            .AddMediatR(c => c.RegisterServicesFromAssemblyContaining<Program>())
            .BuildServiceProvider();
        return serviceProvider;
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>()
            .Build();
        return config;
    }
}



