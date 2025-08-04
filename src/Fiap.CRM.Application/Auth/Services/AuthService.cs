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
using Microsoft.Extensions.Logging;

namespace Fiap.CRM.Application.Auth.Services;

 public class AuthService(
        INotification notification,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IOutboxRepository outboxRepository,
        ILogger<AuthService> logger
        ) : BaseService(notification), IAuthService
    {
        public Task<List<UserResponse>> GetAllAsync() => ExecuteAsync(async () =>
        {
            Func<Task<IEnumerable<Domain.UserAggregate.User>>> fallbackFunc = async () =>
            {
                logger.LogInformation("Cache miss for all users. Fetching from database.");
                var users = await userRepository.GetAllAsync();
                return users;
            };

            var cachedUsers = await cache.GetOrSetAsync(EnumCacheTags.AllUsers, fallbackFunc);

            var allUsers = cachedUsers
                .Select(user => (UserResponse)user)
                .ToList();

            return allUsers;
        });

        public Task<UserResponse> GetAsync(int userId) => ExecuteAsync(async () =>
        {
            var response = new UserResponse();

            var cacheKey = EnumCacheTags.UserId(userId);

            async Task<Domain.UserAggregate.User> fallbackFunc()
            {
                logger.LogInformation("Cache miss for user {UserId}. Fetching from database.", userId);
                var user = await userRepository.GetByIdAsync(userId, noTracking: true);
                return user;
            }

            var cachedUser = await cache.GetOrSetAsync(cacheKey, fallbackFunc);

            if (cachedUser == null)
            {
                _notification.AddNotification("Get User", "User not found", NotificationModel.ENotificationType.NotFound);
                return response;
            }

            response = (UserResponse)cachedUser;

            return response;
        });

        public Task<UserResponse> CreateAsync(CreateUserRequest request) => ExecuteAsync(async () =>
        {
            var response = new UserResponse();

            Validate(request, new CreateUserRequestValidator());

            var email = request.Email.Trim().ToLowerInvariant();

            var exists = await userRepository.ExistAsync(u => u.Email.Address == request.Email.ToLower());
            if (exists)
            {
                _notification.AddNotification("Create User", "Email already registered", NotificationModel.ENotificationType.BusinessRules);
                return response;
            }

            var user = (Domain.UserAggregate.User)request;


            await unitOfWork.BeginTransactionAsync();

            await userRepository.InsertOrUpdateAsync(user);
            await userRepository.SaveChangesAsync();

            var outbox = CreateOutboxMessage("object", user);

            await outboxRepository.InsertOrUpdateAsync(outbox);
            await outboxRepository.SaveChangesAsync();

            await unitOfWork.CommitAsync();

            await cache.RemoveAsync(EnumCacheTags.AllUsers);

            response = (UserResponse)user;

            return response;
        });

        private OutboxMessage CreateOutboxMessage(string type, object content) => new()
        {
            Id = Guid.NewGuid(),
            Type = type,
            Content = JsonSerializer.Serialize(content),
            OccuredOn = DateTime.UtcNow,
        };

        public Task<UserResponse> CreateAdminAsync(CreateUserAdminRequest request) => ExecuteAsync(async () =>
        {
            var response = new UserResponse();

            Validate(request, new CreateUserAdminRequestValidator());

            var email = request.Email.Trim().ToLowerInvariant();

            var exists = await userRepository.ExistAsync(u => u.Email.Address.ToLower() == email);
            if (exists)
            {
                _notification.AddNotification("Create User", "Email already registered", NotificationModel.ENotificationType.BusinessRules);
                return response;
            }

            var user = (Domain.UserAggregate.User)request;


            await unitOfWork.BeginTransactionAsync();

            await userRepository.InsertOrUpdateAsync(user);
            await userRepository.SaveChangesAsync();

            var outbox = CreateOutboxMessage("object", user);

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

            if (user == null)
            {
                _notification.AddNotification("Update User", "User not found", NotificationModel.ENotificationType.NotFound);
                return BaseResponse<object>.Fail(_notification.NotificationModel);
            }

            UpdateUserProperties(user, request);

            await userRepository.UpdateAsync(user);
            await userRepository.SaveChangesAsync();

            await cache.RemoveAsync(EnumCacheTags.AllUsers, EnumCacheTags.UserId(id));

            return BaseResponse<object>.Ok(null);
        });

        private void UpdateUserProperties(Domain.UserAggregate.User user, UpdateUserRequest request)
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

            if (user == null)
            {
                _notification.AddNotification("Delete User", "User Not found", NotificationModel.ENotificationType.NotFound);
                return BaseResponse<object>.Fail(_notification.NotificationModel);
            }

            await userRepository.DeleteAsync(user);
            await userRepository.SaveChangesAsync();

            await cache.RemoveAsync(EnumCacheTags.UserGames(id), EnumCacheTags.UserId(id), EnumCacheTags.AllUsers);

            return BaseResponse<object>.Ok(null);
        });

        public Task<List<UserLibraryGameResponse>> GetGamesByUserAsync(int id) => ExecuteAsync(async () =>
        {
            var cacheKey = EnumCacheTags.UserGames(id);

            async Task<List<LibraryGame>> fallbackFunc()
            {
                logger.LogInformation("Cache miss for user games {UserId}. Fetching from database.", id);

                var user = await userRepository.GetByIdGameUserAsync(id, noTracking: true);

                if (user is null)
                {
                    _notification.AddNotification("Get User", "User not found", NotificationModel.ENotificationType.NotFound);
                    return [];
                }

                return user.LibraryGames.ToList();
            }

            var cachedUserGames = await cache.GetOrSetAsync(cacheKey, fallbackFunc);

            if (cachedUserGames == null)
            {
                _notification.AddNotification("Get User", "User not found", NotificationModel.ENotificationType.NotFound);
                return [];
            }

            return cachedUserGames.Select(lg => (UserLibraryGameResponse)lg).ToList();
        });
    }