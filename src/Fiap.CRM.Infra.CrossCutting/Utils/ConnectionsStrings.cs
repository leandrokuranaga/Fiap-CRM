namespace Fiap.CRM.Infra.Utils;

public class ConnectionStrings
{
    public string CRMConnection { get; set; } = string.Empty;
    public string Redis{ get; set; } = string.Empty;
    public string EventStore { get; set; }

}