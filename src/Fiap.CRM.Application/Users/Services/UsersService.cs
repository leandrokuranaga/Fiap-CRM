using Fiap.Contracts.Events;
using Fiap.CRM.Application.Common;
using Fiap.CRM.Application.Users.Models.Request;
using Fiap.CRM.Application.Users.Models.Response;
using Fiap.CRM.Application.Validators.UsersValidators;
using Fiap.CRM.Domain.OutboxAggregate;
using Fiap.CRM.Domain.SeedWork;
using Fiap.CRM.Domain.SeedWork.Enums;
using Fiap.CRM.Domain.UserAggregate;
using Fiap.CRM.Domain.UserAggregate.Entities;
using Fiap.CRM.Domain.UserAggregate.ValueObjects;
using Fiap.CRM.Infra.CacheService.Services;
using Fiap.CRM.Infra.CrossCutting.Http.FCG;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Fiap.CRM.Application.Users.Services;

 public class UsersService(
        INotification notification,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IOutboxRepository outboxRepository,
        IFCGService fcgService,
        ILogger<UsersService> logger
        ) : BaseService(notification), IUsersService
{
    public Task<List<UserResponse>> GetAllAsync() => ExecuteAsync(async () =>
    {
        await cache.RemoveAsync(EnumCacheTags.AllUsers);
        var list = await cache.GetOrSetAsync(
            EnumCacheTags.AllUsers,
            async () =>
            {
                var users = await userRepository.GetAllWithLibraryAsync();
                return users.Select(u => (UserResponse)u).ToList();
            });

        return list ?? [];
    });

    public Task<UserResponse> GetAsync(int userId) => ExecuteAsync(async () =>
    {
        var cacheKey = EnumCacheTags.UserId(userId);

        var resp = await cache.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                var u = await userRepository.GetWithLibraryAsync(userId);
                return u is null ? null : (UserResponse)u;
            });

        if (resp is null)
        {
            notification.AddNotification("Get User", "User not found", NotificationModel.ENotificationType.NotFound);
            return new UserResponse();
        }

        return resp;
    });

    public Task<UserResponse> CreateAsync(CreateUserRequest request) => ExecuteAsync(async () =>
        {
            var response = new UserResponse();

            Validate(request, new CreateUserRequestValidator());

            var email = request.Email.Trim().ToLowerInvariant();

            var exists = await userRepository.ExistAsync(u => u.Email.Address == request.Email.ToLower());
            if (exists)
            {
                notification.AddNotification("Create User", "Email already registered", NotificationModel.ENotificationType.BusinessRules);
                return response;
            }

            var user = (User)request;

            await unitOfWork.BeginTransactionAsync();

            await userRepository.InsertOrUpdateAsync(user);
            await userRepository.SaveChangesAsync();

            var createUserEvent = new CreateUserIntegrationEvent(user.Id, user.Name, user.Email.Address, (int)user.TypeUser, user.Active);
            var outbox = CreateOutboxMessage(createUserEvent);

            await outboxRepository.InsertOrUpdateAsync(outbox);
            await outboxRepository.SaveChangesAsync();

            await unitOfWork.CommitAsync();

            await cache.RemoveAsync(EnumCacheTags.AllUsers);

            response = (UserResponse)user;

            return response;
        });

    private OutboxMessage CreateOutboxMessage<T>(T content) where T : class => 
        new(typeof(T).AssemblyQualifiedName!, JsonSerializer.Serialize(content), DateTime.UtcNow);

    public Task<UserResponse> CreateAdminAsync(CreateUserAdminRequest request) => ExecuteAsync(async () =>
    {
        var response = new UserResponse();

        Validate(request, new CreateUserAdminRequestValidator());

        var email = request.Email.Trim().ToLowerInvariant();

        var exists = await userRepository.ExistAsync(u => u.Email.Address.ToLower() == email);
        if (exists)
        {
            notification.AddNotification("Create User", "Email already registered", NotificationModel.ENotificationType.BusinessRules);
            return response;
        }

        var user = (User)request;

        await unitOfWork.BeginTransactionAsync();

        await userRepository.InsertOrUpdateAsync(user);
        await userRepository.SaveChangesAsync();

        var createUserEvent = new CreateUserIntegrationEvent(user.Id, user.Name, user.Email.Address, (int)user.TypeUser, user.Active);
        var outbox = CreateOutboxMessage(createUserEvent);

        await outboxRepository.InsertOrUpdateAsync(outbox);
        await outboxRepository.SaveChangesAsync();
        await unitOfWork.CommitAsync();

        await cache.RemoveAsync(EnumCacheTags.AllUsers);

        response = (UserResponse)user;

        return response;
    });

    public Task<BaseResponse<object>> UpdateAsync(int id, UpdateUserRequest request) => ExecuteAsync<BaseResponse<object>>(async () =>
    {
        var response = new UserResponse();

        Validate(request, new UpdateUserRequestValidator());

        var user = await userRepository.GetByIdAsync(id, noTracking: true);

        if (user is null)
        {
            notification.AddNotification("Update User", "User not found", NotificationModel.ENotificationType.NotFound);
            return BaseResponse<object>.Fail(notification.NotificationModel);
        }

        UpdateUserProperties(user, request);

        await userRepository.UpdateAsync(user);
        await userRepository.SaveChangesAsync();

        await cache.RemoveAsync(EnumCacheTags.AllUsers, EnumCacheTags.UserId(id));

        return BaseResponse<object>.Ok(null);
    });

    private void UpdateUserProperties(User user, UpdateUserRequest request)
    {
        user.Name = request.Name ?? user.Name;
        user.Email = new Email(request.Email ?? user.Email.Address);
        user.TypeUser = request.Type ?? user.TypeUser;
        user.Active = request.Active ?? user.Active;

        if (!string.IsNullOrEmpty(request.Password))
            user.Password = new Password(request.Password);
    }

    public Task<BaseResponse<object>> DeleteAsync(int id) => ExecuteAsync(async () =>
    {
        var user = await userRepository.GetByIdAsync(id, noTracking: true);

        if (user is null)
        {
            notification.AddNotification("Delete User", "User Not found", NotificationModel.ENotificationType.NotFound);
            return BaseResponse<object>.Fail(notification.NotificationModel);
        }

        await userRepository.DeleteAsync(user);
        await userRepository.SaveChangesAsync();

        await cache.RemoveAsync(EnumCacheTags.UserGames(id), EnumCacheTags.UserId(id), EnumCacheTags.AllUsers);

        return BaseResponse<object>.Ok(null);
    });

    public Task<List<UserLibraryGameResponse>> GetGamesByUserAsync(int id) => ExecuteAsync(async () =>
    {
        var cacheKey = EnumCacheTags.UserGames(id);

        async Task<List<UserLibraryGameResponse>> fallbackFunc()
        {
            logger.LogInformation("Cache miss for user games {UserId}. Fetching from database.", id);

            var user = await userRepository.GetWithLibraryAsync(id);
            
            if (user is null)
                return [];

            var gamesResponse = await fcgService.GetAllGamesAsync();
            
            if (!gamesResponse.Success || gamesResponse.Data == null)
                return [];
            
            var gamesDict = gamesResponse.Data.ToDictionary(g => g.Id, g => g);

            var userGames = new List<UserLibraryGameResponse>();

            foreach (var libraryGame in user.LibraryGames)
            {
                if (gamesDict.TryGetValue(libraryGame.GameId, out var gameData))
                {
                    userGames.Add(new UserLibraryGameResponse
                    {
                        GameId = gameData.Id,
                        Name = gameData.Name,
                        Genre = gameData.Genre,
                        OriginalPrice = gameData.Price,
                        OriginalCurrency = "BRL",
                        PricePaid = libraryGame.PricePaid.Value,
                        PurchaseCurrency = libraryGame.PricePaid.Currency,
                        PurchaseDate = libraryGame.PurchaseDate
                    });
                }
            }

            return userGames;
        }

        var cachedUserGames = await cache.GetOrSetAsync(cacheKey, fallbackFunc);

        if (cachedUserGames is null || !cachedUserGames.Any())
        {
            notification.AddNotification("Get User", "User not found or has no games", NotificationModel.ENotificationType.NotFound);
            return [];
        }

        return cachedUserGames;
    });
}
