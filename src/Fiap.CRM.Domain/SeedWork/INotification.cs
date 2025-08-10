namespace Fiap.CRM.Domain.SeedWork;

public interface INotification
{
    NotificationModel NotificationModel { get; }
    bool HasNotification { get; }
    void AddNotification(string key, string message, NotificationModel.ENotificationType notificationType);
}