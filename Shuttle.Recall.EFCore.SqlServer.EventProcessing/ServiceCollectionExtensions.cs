using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Recall.EFCore.SqlServer.EventProcessing;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqlServerEventProcessing(this IServiceCollection services, Action<SqlServerEventProcessingBuilder>? builder = null)
    {
        var eventProcessingBuilder = new SqlServerEventProcessingBuilder(Guard.AgainstNull(services));

        builder?.Invoke(eventProcessingBuilder);

        services.AddSingleton<IValidateOptions<SqlServerEventProcessingOptions>, SqlServerEventProcessingOptionsValidator>();
        services.AddSingleton<IPrimitiveEventQuery, PrimitiveEventQuery>();
        services.AddSingleton<IProjectionQuery, ProjectionQuery>();
        services.AddSingleton<IProjectionService, ProjectionService>();
        services.AddSingleton<EventProcessingStartupObserver>();

        services.AddSingleton<DbContextObserver, DbContextObserver>();

        services.AddOptions<SqlServerEventProcessingOptions>().Configure(options =>
        {
            options.ConnectionStringName = eventProcessingBuilder.Options.ConnectionStringName;
            options.Schema = eventProcessingBuilder.Options.Schema;
            options.MigrationsHistoryTableName = eventProcessingBuilder.Options.MigrationsHistoryTableName;
            options.CommandTimeout = eventProcessingBuilder.Options.CommandTimeout;
        });

        services.AddDbContextFactory<EventProcessingDbContext>((provider, dbContextFactoryBuilder) =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();

            var connectionString = configuration.GetConnectionString(eventProcessingBuilder.Options.ConnectionStringName);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException(string.Format(Resources.ConnectionStringNameException, eventProcessingBuilder.Options.ConnectionStringName));
            }

            dbContextFactoryBuilder.UseSqlServer(connectionString, sqlServerOptions =>
            {
                sqlServerOptions.CommandTimeout(eventProcessingBuilder.Options.CommandTimeout);
            });
        });

        services.AddHostedService<EventProcessingHostedService>();

        return services;
    }
}