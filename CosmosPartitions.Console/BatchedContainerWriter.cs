using Microsoft.Azure.Cosmos;
using Serilog;

namespace CosmosPartitions.Console;

public class BatchedContainerWriter
{
    private readonly int _batchSize;

    public BatchedContainerWriter(int batchSize=100)
    {
        _batchSize = batchSize;
    }

    public async Task BatchedWrite(Container container, IEnumerable<UserProductEvent> events, CancellationToken ct)
    {
        using var enumerator = events.GetEnumerator();
        int totalWritten = 0;

        while (enumerator.MoveNext())
        {
            int batched = 0;
            var tasks = new List<Task>();
            do
            {
                tasks.Add(container.CreateItemAsync(enumerator.Current, cancellationToken: ct)
                    .ContinueWith(itemResponse =>
                    {
                        if (!itemResponse.IsCompletedSuccessfully)
                        {
                            AggregateException innerExceptions = itemResponse.Exception.Flatten();
                            if (innerExceptions.InnerExceptions.FirstOrDefault(innerEx => innerEx is CosmosException) is CosmosException cosmosException)
                            {
                                Log.Information($"Received {cosmosException.StatusCode} ({cosmosException.Message}).");
                            }
                            else
                            {
                                Log.Information($"Exception {innerExceptions.InnerExceptions.FirstOrDefault()}.");
                            }
                        }
                    }, ct));
                batched++;
                totalWritten++;
            } while (batched < _batchSize && enumerator.MoveNext());
            await Task.WhenAll(tasks);
            Log.Information("{Count} items written", totalWritten);
        }
    }


}