using Fcs.UI.Domain.Events;

namespace Fcs.UI.Application.Interfaces;

public interface IEventStore
{
    Task AppendAsync<T>(T domainEvent, Guid aggregateId) where T : IDomainEvent;
    Task<IEnumerable<IDomainEvent>> GetEventsAsync(Guid aggregateId);
}
