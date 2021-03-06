using System;
using System.Collections.Generic;
using Loveboat.Domain.Aggregates;
using NServiceBus;

namespace Loveboat.Domain.Infrastructure
{
    public abstract class Repository<T> : IRepository<T> where T : AggregateBase, new()
    {
        private readonly IBus bus;

        protected Repository(IBus bus)
        {
            if (bus == null) throw new ArgumentNullException("bus");
            this.bus = bus;
        }
        
        public T GetById(Guid id)
        {
            var aggregate = new T();
            var eventsForAggreate = GetEventsFor(id);
            aggregate.LoadFromEvents(id, eventsForAggreate);
            return aggregate;
        }

        public void Save(T aggregate)
        {
            foreach (var uncommitedEvent in aggregate.UncommitedEvents)
            {
                FakeEventStore.Events[aggregate.Id].Add(uncommitedEvent);
                bus.Publish(uncommitedEvent);
            }
        }

        private static IEnumerable<IEvent> GetEventsFor(Guid id)
        {
            return FakeEventStore.Events[id];
        }
    }
}