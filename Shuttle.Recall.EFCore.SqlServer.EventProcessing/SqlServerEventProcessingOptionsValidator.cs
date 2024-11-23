using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Recall.EFCore.SqlServer.EventProcessing;

public class SqlServerEventProcessingOptionsValidator : IValidateOptions<SqlServerEventProcessingOptions>
{
    public ValidateOptionsResult Validate(string? name, SqlServerEventProcessingOptions options)
    {
        Guard.AgainstNull(options);

        if (string.IsNullOrWhiteSpace(options.ConnectionStringName))
        {
            return ValidateOptionsResult.Fail(Resources.ConnectionStringOptionException);
        }

        return ValidateOptionsResult.Success;
    }
}