using AuthApi.Contracts.Responses;
using AuthApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _dbContext;
        public UserRepository(AuthDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(UserEntity user)
        {
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> CheckExistenceByEmail(string email)
        {
            return await _dbContext.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<UserEntity?> GetByEmailAsync(string email)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(user => user.Email == email);
        }

        public async Task<UserInfo> GetByIdAsync(Guid userId)
        {
            return await _dbContext.Users.Where(user => user.Id == userId)
                .Select(u => new UserInfo(u.Id, u.Email))
                .FirstOrDefaultAsync();
        }
    }
}
