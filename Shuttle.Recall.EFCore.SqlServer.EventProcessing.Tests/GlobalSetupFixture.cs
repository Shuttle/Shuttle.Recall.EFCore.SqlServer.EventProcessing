using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Shuttle.Extensions.EFCore;
using Shuttle.Recall.EFCore.SqlServer.Storage;

namespace Shuttle.Recall.EFCore.SqlServer.EventProcessing.Tests;

[SetUpFixture]
public class GlobalSetupFixture
{
    [OneTimeSetUp]
    public void GlobalSetup()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var storageOptions = configuration.GetSection(SqlServerStorageOptions.SectionName).Get<SqlServerStorageOptions>()!;

        if (storageOptions == null)
        {
            throw new InvalidOperationException($"Could not find a section called '{SqlServerStorageOptions.SectionName}'.");
        }

        var storageDbContextOptions = new DbContextOptionsBuilder<StorageDbContext>()
            .UseSqlServer(configuration.GetConnectionString(storageOptions.ConnectionStringName), sqlServerBuilder =>
                {
                    sqlServerBuilder.CommandTimeout(300);
                    sqlServerBuilder.MigrationsHistoryTable(storageOptions.MigrationsHistoryTableName, storageOptions.Schema);
                }
            )
            .ReplaceService<IMigrationsAssembly, SchemaMigrationsAssembly>()
            .Options;

        using (var dbContext = new StorageDbContext(Options.Create(storageOptions), storageDbContextOptions))
        {
            dbContext.Database.Migrate();
        }

        var eventProcessingOptions = configuration.GetSection(SqlServerEventProcessingOptions.SectionName).Get<SqlServerEventProcessingOptions>()!;

        if (eventProcessingOptions == null)
        {
            throw new InvalidOperationException($"Could not find a section called '{SqlServerEventProcessingOptions.SectionName}'.");
        }

        var eventProcessingDbContextOptions = new DbContextOptionsBuilder<EventProcessingDbContext>()
            .UseSqlServer(configuration.GetConnectionString(eventProcessingOptions.ConnectionStringName), sqlServerBuilder =>
                {
                    sqlServerBuilder.CommandTimeout(300);
                    sqlServerBuilder.MigrationsHistoryTable(eventProcessingOptions.MigrationsHistoryTableName, eventProcessingOptions.Schema);
                }
            )
            .ReplaceService<IMigrationsAssembly, SchemaMigrationsAssembly>()
            .Options;

        using (var dbContext = new EventProcessingDbContext(Options.Create(eventProcessingOptions), eventProcessingDbContextOptions))
        {
            dbContext.Database.Migrate();
        }
    }
}