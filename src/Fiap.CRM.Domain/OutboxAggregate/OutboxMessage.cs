using Abp.Domain.Entities;

namespace Fiap.CRM.Domain.OutboxAggregate;

public class OutboxMessage : Entity<Guid>
{
    public string Type { get; set; }
    public string Content { get; set; }
    public DateTime OccuredOn { get; set; }
    public DateTime? ProcessedOn { get; set; }

    public OutboxMessage()
    {
            
    }
        
    public OutboxMessage(string type, string content, DateTime occuredOn, DateTime? processedOn = null)
    {
        Type = type;
        Content = content;
        OccuredOn = occuredOn;
        ProcessedOn = processedOn;
    }
}