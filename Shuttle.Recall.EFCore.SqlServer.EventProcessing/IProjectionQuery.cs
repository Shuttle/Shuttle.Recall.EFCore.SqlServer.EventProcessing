namespace Shuttle.Recall.EFCore.SqlServer.EventProcessing;

public interface IProjectionQuery
{
    Task SetSequenceNumberAsync(string projectionName, long sequenceNumber);
    ValueTask<long> GetSequenceNumberAsync(string projectionName);
}