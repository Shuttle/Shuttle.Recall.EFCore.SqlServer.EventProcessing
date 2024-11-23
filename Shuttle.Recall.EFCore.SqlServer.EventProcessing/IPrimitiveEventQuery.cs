namespace Shuttle.Recall.EFCore.SqlServer.EventProcessing;

public interface IPrimitiveEventQuery
{
    Task<IEnumerable<Storage.Models.PrimitiveEvent>> SearchAsync(Storage.Models.PrimitiveEvent.Specification specification);
}