using System.Diagnostics;
using System.Xml;
using MediatR;
using Microsoft.Azure.Cosmos;
using Serilog;

namespace CosmosPartitions.Console.Commands;

public class CreateHierarchicalKeyedCollection: IRequest
{
    
}

public class CreateHierarchicalKeyedCollectionHandler : IRequestHandler<CreateHierarchicalKeyedCollection>
{
    private readonly CosmosClient _cosmosClient;

    public CreateHierarchicalKeyedCollectionHandler(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    public async Task Handle(CreateHierarchicalKeyedCollection request, CancellationToken cancellationToken)
    {
        var db = _cosmosClient.GetDatabase("cosmos-partition-tests-db");

        var containerProperties = new ContainerProperties(id: "hierarchical-key",
            partitionKeyPaths: new List<string>() { "/OrgId", "/UserId" });
        
        containerProperties.IndexingPolicy = new IndexingPolicy()
        {
            IndexingMode = IndexingMode.Consistent,
            ExcludedPaths = { new ExcludedPath() { Path = "/*" } }
        };
        var containerResponse = await db.CreateContainerIfNotExistsAsync(containerProperties: containerProperties, throughput: 50000, cancellationToken: cancellationToken);
        var container = containerResponse.Container;
        
        var data = new Data();
        var events = data.GetEvents(250000);

        var writer = new BatchedContainerWriter();
        await writer.BatchedWrite(container, events, cancellationToken);
        
        Log.Information("CreateHierarchicalKeyedCollection Command Complete");
    }
}