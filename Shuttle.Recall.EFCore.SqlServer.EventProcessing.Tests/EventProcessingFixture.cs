using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Shuttle.Recall.EFCore.SqlServer.Storage;
using Shuttle.Recall.Tests;

namespace Shuttle.Recall.EFCore.SqlServer.EventProcessing.Tests;

public class EventProcessingFixture : RecallFixture
{
    [Test]
    public async Task Should_be_able_to_process_events_async()
    {
        var services = SqlConfiguration.GetServiceCollection();

        var serviceProvider = services.BuildServiceProvider();
        var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<StorageDbContext>>();
        var options = serviceProvider.GetRequiredService<IOptions<SqlServerEventProcessingOptions>>().Value;

        await using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        {
            await dbContext.Database.ExecuteSqlRawAsync($"delete from [{options.Schema}].[PrimitiveEvent] where Id in ('{OrderId}', '{OrderProcessId}')");
            await dbContext.Database.ExecuteSqlRawAsync($"delete from [{options.Schema}].[PrimitiveEventJournal] where Id in ('{OrderId}', '{OrderProcessId}')");
        }

        await ExerciseEventProcessingAsync(services, builder =>
        {
            builder.Options.ProjectionThreadCount = 1;
        }, handlerTimeoutSeconds: 300);
    }
}