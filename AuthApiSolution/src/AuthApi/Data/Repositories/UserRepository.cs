using AuthApi.Contracts.Responses;
using AuthApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _dbContext;
    public UserRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(UserEntity user, CancellationToken cancellationToken)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> CheckExistenceByEmail(string email, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<UserEntity> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    public async Task<UserInfo> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.Where(user => user.Id == userId)
            .Select(u => new UserInfo(u.Id, u.Email, u.Name, u.LastName))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<UserInfo>> GetAllUsers(CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .Select(u => new UserInfo(u.Id, u.Email, u.Name, u.LastName))
            .ToListAsync(cancellationToken);
    }
}
