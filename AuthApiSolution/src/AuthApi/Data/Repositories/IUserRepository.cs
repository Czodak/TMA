using AuthApi.Contracts.Responses;
using AuthApi.Data.Entities;

namespace AuthApi.Data.Repositories;

public interface IUserRepository
{
    Task<UserEntity> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task AddAsync(UserEntity user, CancellationToken cancellationToken);
    Task<bool> CheckExistenceByEmail(string email, CancellationToken cancellationToken);
    Task<UserInfo> GetByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<List<UserInfo>> GetAllUsers(CancellationToken cancellationToken);
}
