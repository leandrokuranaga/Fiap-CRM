namespace Fiap.CRM.Domain.Events
{
	public record EventStored : IEventSourcing
	{
		public Guid Id { get; set; }
		public string AggregateId { get; set; } = string.Empty;
		public string StreamName { get; set; } = string.Empty;
		public string Type { get; set; } = string.Empty;
		public string Data { get; set; } = string.Empty;
		public DateTime OccurredOn { get; set; }
	}
}
