using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Shuttle.Extensions.EFCore;
using Shuttle.Recall.EFCore.SqlServer.Storage;

namespace Shuttle.Recall.EFCore.SqlServer.EventProcessing;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<EventProcessingDbContext>
{
    public EventProcessingDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddCommandLine(args)
            .Build();

        var sqlServerEventProcessingOptions = configuration.GetSection(SqlServerEventProcessingOptions.SectionName).Get<SqlServerEventProcessingOptions>()!;

        if (sqlServerEventProcessingOptions == null)
        {
            throw new InvalidOperationException($"Could not find a section called '{SqlServerEventProcessingOptions.SectionName}' in the configuration.");
        }

        var schemaOverride = configuration["SchemaOverride"];

        if (!string.IsNullOrWhiteSpace(schemaOverride))
        {
            Console.WriteLine(@$"[schema-override] : original schema = '{sqlServerEventProcessingOptions.Schema}' / schema override = '{schemaOverride}'");

            sqlServerEventProcessingOptions.Schema = schemaOverride;
        }

        var optionsBuilder = new DbContextOptionsBuilder<EventProcessingDbContext>();

        optionsBuilder
            .UseSqlServer(configuration.GetConnectionString(sqlServerEventProcessingOptions.ConnectionStringName),
                builder => builder.MigrationsHistoryTable(sqlServerEventProcessingOptions.MigrationsHistoryTableName, sqlServerEventProcessingOptions.Schema));

        optionsBuilder.ReplaceService<IMigrationsAssembly, SchemaMigrationsAssembly>();

        return new(Options.Create(sqlServerEventProcessingOptions), optionsBuilder.Options);
    }
}