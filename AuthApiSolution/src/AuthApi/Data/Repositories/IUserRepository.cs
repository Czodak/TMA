using AuthApi.Contracts.Responses;
using AuthApi.Data.Entities;

namespace AuthApi.Data.Repositories
{
    public interface IUserRepository
    {
        Task<UserEntity?> GetByEmailAsync(string email);
        Task AddAsync(UserEntity user);
        Task<bool> CheckExistenceByEmail(string email);
        Task<UserInfo> GetByIdAsync(Guid userId);

        Task<List<UserInfo>> GetAllUsers();        
    }
}
