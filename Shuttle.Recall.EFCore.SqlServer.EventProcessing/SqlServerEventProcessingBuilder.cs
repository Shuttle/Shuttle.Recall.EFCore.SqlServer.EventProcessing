using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Recall.EFCore.SqlServer.EventProcessing;

public class SqlServerEventProcessingBuilder
{
    private SqlServerEventProcessingOptions _sqlEventProcessingOptions = new();

    public SqlServerEventProcessingBuilder(IServiceCollection services)
    {
        Services = Guard.AgainstNull(services);
    }

    public SqlServerEventProcessingOptions Options
    {
        get => _sqlEventProcessingOptions;
        set => _sqlEventProcessingOptions = Guard.AgainstNull(value);
    }

    public IServiceCollection Services { get; }
}