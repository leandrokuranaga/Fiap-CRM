using Fiap.CRM.Domain.UserAggregate;
using Fiap.CRM.Infra.Data.Repositories.Base;

namespace Fiap.CRM.Infra.Data.Repositories;

public class UserRepository(Context context) : BaseRepository<User>(context), IUserRepository
{
}