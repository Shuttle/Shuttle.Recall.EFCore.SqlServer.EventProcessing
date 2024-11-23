namespace Shuttle.Recall.EFCore.SqlServer.EventProcessing;

public class SqlServerEventProcessingOptions
{
    public const string SectionName = "Shuttle:EventStore:SqlServer:EventProcessing";

    public string ConnectionStringName { get; set; } = string.Empty;
    public string Schema { get; set; } = "dbo";
    public string MigrationsHistoryTableName { get; set; } = "__EventProcessingMigrationsHistory";
    public int CommandTimeout { get; set; } = 30;
}