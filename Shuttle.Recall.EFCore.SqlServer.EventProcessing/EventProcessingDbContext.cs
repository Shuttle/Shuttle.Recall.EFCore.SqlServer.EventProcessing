using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Extensions.EFCore;
using Shuttle.Recall.EFCore.SqlServer.EventProcessing.Models;

namespace Shuttle.Recall.EFCore.SqlServer.EventProcessing;

public class EventProcessingDbContext : DbContext, IDbContextSchema
{
    private readonly SqlServerEventProcessingOptions _sqlServerEventProcessingOptions;

    public EventProcessingDbContext(IOptions<SqlServerEventProcessingOptions> sqlServerEventProcessingOptions, DbContextOptions<EventProcessingDbContext> options) : base(options)
    {
        _sqlServerEventProcessingOptions = Guard.AgainstNull(Guard.AgainstNull(sqlServerEventProcessingOptions).Value);
    }

    public DbSet<Models.Projection> Projections { get; set; } = null!;
    public DbSet<ProjectionJournal> ProjectionJournals { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(_sqlServerEventProcessingOptions.Schema);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            entityType.SetTableName(entityType.DisplayName());
        }
    }

    public string Schema => _sqlServerEventProcessingOptions.Schema;
}