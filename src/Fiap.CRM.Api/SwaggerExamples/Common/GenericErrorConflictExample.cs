using System.Diagnostics.CodeAnalysis;
using Fiap.CRM.Application.Common;
using Fiap.CRM.Domain.SeedWork;

namespace Fiap.CRM.Api.SwaggerExamples.Common;

[ExcludeFromCodeCoverage]
public class GenericErrorConflictExample : IExamplesProvider<BaseResponse<object>>
{
    public BaseResponse<object> GetExamples()
    {
        var notification = new NotificationModel
        {
            NotificationType = NotificationModel.ENotificationType.BusinessRules
        };

        notification.AddMessage("Field", "Field already in use");
        notification.AddMessage("Conflict", "This resource already exists and cannot be duplicated");

        return BaseResponse<object>.Fail(notification);
    }
}