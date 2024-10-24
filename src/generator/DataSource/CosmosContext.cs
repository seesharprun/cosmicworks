using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;

namespace Microsoft.Samples.Cosmos.NoSQL.CosmicWorks.Generator.DataSource;

public sealed class CosmosContext : ICosmosContext
{
    private const int _databaseThroughput = 400;

    public async Task SeedDataAsync<T>(string connectionString, string databaseName, ContainerProperties containerProperties, IEnumerable<T> items, Action<string> onCreated)
    {
        using CosmosClient client = new CosmosClientBuilder(connectionString)
            .WithSerializerOptions(new CosmosSerializationOptions()
            {
                IgnoreNullValues = true,
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
            })
            .WithBulkExecution(true)
            .WithThrottlingRetryOptions(TimeSpan.FromSeconds(30), 30)
            .Build();

        AccountProperties accountProperties = await client.ReadAccountAsync();

        Database database = await client.CreateDatabaseIfNotExistsAsync(
            id: databaseName,
            throughput: _databaseThroughput
        );

        Container container = await database.CreateContainerIfNotExistsAsync(
            containerProperties: containerProperties
        );

        if (database is not null && container is not null)
        {
            await database.ReplaceThroughputAsync(4000);

            List<Task> tasks = new(items.Count());
            foreach (var item in items)
            {
                tasks.Add(
                    container.UpsertItemAsync(item)
                        .ContinueWith(r =>
                            {
                                if (r.IsCompletedSuccessfully)
                                {
                                    onCreated($"{r.Result.Resource}");
                                }
                            }
                        )
                );
            }
            await Task.WhenAll(tasks);

            await database.ReplaceThroughputAsync(400);
        }
    }
}