using MediatR;
using Microsoft.Azure.Cosmos;
using Serilog;

namespace CosmosPartitions.Console.Commands;

public class CreateOrgIdKeyedCollection: IRequest
{
    
}

public class CreateOrgIdKeyedCollectionHandler : IRequestHandler<CreateOrgIdKeyedCollection>
{
    private readonly CosmosClient _cosmosClient;

    public CreateOrgIdKeyedCollectionHandler(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    public async Task Handle(CreateOrgIdKeyedCollection request, CancellationToken cancellationToken)
    {
        var db = _cosmosClient.GetDatabase("cosmos-partition-tests-db");
        
        var containerResponse = await db.DefineContainer("org-id-key", "/OrgId")
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
        
        Log.Information("CreateOrgIdKeyedCollection Command Complete");
    }
}