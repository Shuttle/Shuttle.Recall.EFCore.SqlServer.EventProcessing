using System.Data.Common;
using Castle.Core.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shuttle.Extensions.EFCore;
using Shuttle.Recall.EFCore.SqlServer.Storage;
using Shuttle.Recall.Logging;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Shuttle.Recall.EFCore.SqlServer.EventProcessing.Tests;

[SetUpFixture]
public class SqlConfiguration
{
    public static IServiceCollection GetServiceCollection()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var storageOptions = configuration.GetSection(SqlServerStorageOptions.SectionName).Get<SqlServerStorageOptions>()!;
        var eventProcessingOptions = configuration.GetSection(SqlServerEventProcessingOptions.SectionName).Get<SqlServerEventProcessingOptions>()!;

        var services = new ServiceCollection();

        services
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton<IDbContextService, DbContextService>()
            .AddSqlServerEventStorage(builder =>
            {
                builder.Options.ConnectionStringName = storageOptions.ConnectionStringName;
                builder.Options.Schema = storageOptions.Schema;
                builder.Options.MigrationsHistoryTableName = storageOptions.MigrationsHistoryTableName;
            })
            .AddSqlServerEventProcessing(builder =>
            {
                builder.Options.ConnectionStringName = eventProcessingOptions.ConnectionStringName;
                builder.Options.Schema = eventProcessingOptions.Schema;
                builder.Options.MigrationsHistoryTableName = eventProcessingOptions.MigrationsHistoryTableName;
            })
            .AddEventStoreLogging();

        services.AddDbContextFactory<StorageDbContext>(builder =>
        {
            var connectionString = configuration.GetConnectionString(storageOptions.ConnectionStringName);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException($"Could not find a connection string called '{storageOptions.ConnectionStringName}'.");
            }

            builder.UseSqlServer(connectionString, sqlServerBuilder =>
            {
                sqlServerBuilder.CommandTimeout(300);
                sqlServerBuilder.MigrationsHistoryTable(storageOptions.MigrationsHistoryTableName, storageOptions.Schema);
            });

            builder.ReplaceService<IMigrationsAssembly, SchemaMigrationsAssembly>();
        });

        services.AddDbContextFactory<EventProcessingDbContext>(builder =>
        {
            var connectionString = configuration.GetConnectionString(eventProcessingOptions.ConnectionStringName);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException($"Could not find a connection string called '{eventProcessingOptions.ConnectionStringName}'.");
            }

            builder.UseSqlServer(connectionString, sqlServerBuilder =>
            {
                sqlServerBuilder.CommandTimeout(300);
                sqlServerBuilder.MigrationsHistoryTable(eventProcessingOptions.MigrationsHistoryTableName, eventProcessingOptions.Schema);
            });

            builder.ReplaceService<IMigrationsAssembly, SchemaMigrationsAssembly>();
        });

        return services;
    }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", SqlClientFactory.Instance);
    }
}