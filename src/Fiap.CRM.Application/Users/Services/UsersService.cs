using System.Text.Json;
using Fiap.Contracts.Events;
using Fiap.CRM.Application.Common;
using Fiap.CRM.Application.Users.Models.Request;
using Fiap.CRM.Application.Users.Models.Response;
using Fiap.CRM.Application.Validators.UsersValidators;
using Fiap.CRM.Domain.OutboxAggregate;
using Fiap.CRM.Domain.SeedWork;
using Fiap.CRM.Domain.SeedWork.Enums;
using Fiap.CRM.Domain.UserAggregate;
using Fiap.CRM.Domain.UserAggregate.ValueObjects;
using Fiap.CRM.Infra.CacheService.Services;
using Fiap.CRM.Infra.CrossCutting.Http.Keycloak.Service;
using Microsoft.Extensions.Logging;

namespace Fiap.CRM.Application.Users.Services;

 public class UsersService(
        INotification notification,
        IUserRepository userRepository,
        IKeycloakService keycloakService,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IOutboxRepository outboxRepository,
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

            var kcResult = await keycloakService.CreateUserAsync(user.Email.Address, user.Name, request.Password, user.TypeUser.ToString());
            if (!kcResult)
            {
                notification.AddNotification("Keycloak", "Erro ao criar usuário no Keycloak", NotificationModel.ENotificationType.BusinessRules);
                await unitOfWork.RollbackAsync();
                return response;
            }

            var outbox = CreateOutboxMessage("object", user);

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

            var kcResult = await keycloakService.CreateUserAsync(user.Email.Address, user.Name, request.Password, request.TypeUser.ToString());
            if (!kcResult)
            {
                notification.AddNotification("Keycloak", "Erro ao criar usuário no Keycloak", NotificationModel.ENotificationType.BusinessRules);
                await unitOfWork.RollbackAsync();
                return response;
            }


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

            var kcDeleted = await keycloakService.DeleteUserAsync(user.Email.Address);
            if (!kcDeleted)
            {
                notification.AddNotification("Keycloak", "Erro ao deletar usuário no Keycloak", NotificationModel.ENotificationType.BusinessRules);
                return BaseResponse<object>.Fail(notification.NotificationModel);
            }

            await userRepository.DeleteAsync(user);
            await userRepository.SaveChangesAsync();

            await cache.RemoveAsync(EnumCacheTags.UserGames(id), EnumCacheTags.UserId(id), EnumCacheTags.AllUsers);

            return BaseResponse<object>.Ok(null);
        });
    }
