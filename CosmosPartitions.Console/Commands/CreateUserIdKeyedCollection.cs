using MediatR;
using Microsoft.Azure.Cosmos;
using Serilog;

namespace CosmosPartitions.Console.Commands;

public class CreateUserIdKeyedCollection: IRequest
{
    
}


public class CreateUserIdKeyedCollectionHandler : IRequestHandler<CreateUserIdKeyedCollection>
{
    private readonly CosmosClient _cosmosClient;

    public CreateUserIdKeyedCollectionHandler(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    public async Task Handle(CreateUserIdKeyedCollection request, CancellationToken cancellationToken)
    {
        var db = _cosmosClient.GetDatabase("cosmos-partition-tests-db");
        
        var containerResponse = await db.DefineContainer("user-id-key", "/UserId")
            .WithIndexingPolicy()
                .WithIndexingMode(IndexingMode.Consistent)
                .WithIncludedPaths()
                    .Attach()
                .WithExcludedPaths()
                    .Path("/*")
                    .Attach()
            .Attach()
            .CreateIfNotExistsAsync(50000, cancellationToken);
       
        var container = containerResponse.Container;
        
        var data = new Data();
        var events = data.GetEvents(250000);
        
        Log.Information("Writing to {Container}", container.Id);

        var writer = new BatchedContainerWriter();
        await writer.BatchedWrite(container, events, cancellationToken);
        
        Log.Information("CreateUserIdKeyedCollection Command Complete");
    }
}