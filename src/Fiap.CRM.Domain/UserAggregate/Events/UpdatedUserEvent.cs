using Fiap.CRM.Domain.SeedWork;
using System.Text.Json;

namespace Fiap.CRM.Domain.UserAggregate.Events;

public record UpdatedUserEvent : StoredEvent
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;

    public UpdatedUserEvent()
    {

    }

    public UpdatedUserEvent(User user)
    {
        Id = Guid.NewGuid();
        UserId = user.Id;
        Name = user.Name;
        OccurredOn = DateTime.UtcNow;
        StreamName = $"{nameof(User)}-{user.Id}";
        Type = nameof(User);
        Data = JsonSerializer.Serialize(new
        {
            user.Id,
            user.Name,
            user.Email,
            TypeUser = (int)user.TypeUser,
            user.Active
        });
    }
}