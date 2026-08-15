namespace RescueLink.Domain.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; protected set; }
        private readonly List<IDomainEvent> _domainEvents = [];

        public IReadOnlyCollection<IDomainEvent> DomainEvents =>
            _domainEvents.AsReadOnly();

        protected void RaiseDomainEvent(IDomainEvent domainEvent)
        {
            ArgumentNullException.ThrowIfNull(domainEvent);

            _domainEvents.Add(domainEvent);
        }

        public IReadOnlyCollection<IDomainEvent> ClearDomainEvents()
        {
            var domainEvents = _domainEvents.ToArray();

            _domainEvents.Clear();

            return domainEvents;
        }
    }
}
