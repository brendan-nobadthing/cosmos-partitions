using MediatR;
using Microsoft.Azure.Cosmos;
using Serilog;

namespace CosmosPartitions.Console.Commands;

public class CreateIdKeyedCollection: IRequest
{
    
}


public class CreateIdKeyedCollectionHandler : IRequestHandler<CreateIdKeyedCollection>
{
    private readonly CosmosClient _cosmosClient;

    public CreateIdKeyedCollectionHandler(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    public async Task Handle(CreateIdKeyedCollection request, CancellationToken cancellationToken)
    {
        var db = _cosmosClient.GetDatabase("cosmos-partition-tests-db");
        var containerResponse = await db.DefineContainer("id-key", "/id")
            .WithIndexingPolicy()
                .WithIndexingMode(IndexingMode.Consistent)
                .WithIncludedPaths()
                    .Attach()
                .WithExcludedPaths()
                    .Path("/*")
                    .Attach()
            .Attach()
            .CreateIfNotExistsAsync(5000, cancellationToken);
        var container = containerResponse.Container;
        
        var data = new Data();
        var events = data.GetEvents(250000);

        var writer = new BatchedContainerWriter();
        await writer.BatchedWrite(container, events, cancellationToken);
        
        Log.Information("CreateIdKeyedCollection Command Complete");
    }
}