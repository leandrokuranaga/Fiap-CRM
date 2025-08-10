namespace Fiap.CRM.Domain.SeedWork.Enums;

public static class EnumCacheTags
{
    public static string FiapPrefix => "Fiap:";

    // Users
    public static string UsersPrefix => $"{FiapPrefix}Users:";
    public static string AllUsers => $"{UsersPrefix}All";
    public static string UserId(int id) => $"{UsersPrefix}Id:{id}";
    public static string UserGames(int userId) => $"{UsersPrefix}Id:{userId}:Games";
}