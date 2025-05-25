// Copyright (c) Microsoft Corporation. All rights reserved.
namespace Microsoft.Samples.Cosmos.NoSQL.CosmicWorks.Generator;

/// <summary>
/// A data generator for Azure Cosmos DB for NoSQL that generates items of a generic type.
/// </summary>
public class CosmosDataGenerator<T>(
    IDataSource<T> dataSource,
    ICosmosDataService<T> cosmosDataService
) : ICosmosDataGenerator<T> where T : IItem
{
    private readonly int standardDatabaseThroughput = 400;

    private readonly int[] scaledDatabaseThroughputs = [
        10000, // Maximum amount of throughput the tool will attempt to set.
        4000, // Ceiling for standard accounts with throughput limit set.
        1000 // Ceiling for free-tier accounts with throughput limit set.
    ];

    /// <inheritdoc />
    public async Task GenerateAsync(ConnectionOptions options, string databaseName, string containerName, int? count = null, bool disableHierarchicalPartitionKeys = false, Action<T>? onItemCreate = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        onItemCreate ??= _ => { };

        IReadOnlyList<T> seedItems = dataSource.GetItems(count);

        if (options.Type == ConnectionType.ResourceOwnerPasswordCredential)
        {
            string[] partitionKeys = [.. seedItems[0].PartitionKeys];

            bool serverless = await cosmosDataService.ProvisionResourcesAsync(
                options,
                databaseName,
                containerName,
                partitionKeys,
                disableHierarchicalPartitionKeys
            );

            if (!serverless)
            {
                // Scale up to start operations at a higher throughput.
                await cosmosDataService.UpdateThroughputAsync(
                    options,
                    databaseName,
                    containerName,
                    scaledDatabaseThroughputs
                );
            }

            await cosmosDataService.SeedDataAsync(
                    options,
                    databaseName,
                    containerName,
                    seedItems,
                    onItemCreate
                );

            if (!serverless)
            {
                // Scale down to the standard throughput.
                await cosmosDataService.UpdateThroughputAsync(
                    options,
                    databaseName,
                    containerName,
                    [standardDatabaseThroughput]
                );
            }
        }
        else
        {
            await cosmosDataService.SeedDataAsync(
                options,
                databaseName,
                containerName,
                seedItems,
                onItemCreate
            );
        }
    }
}