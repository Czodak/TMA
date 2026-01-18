using Microsoft.Extensions.Logging;
using TaskApi.Common.HttpClients.Auth;

namespace TaskApi.BusinessLogic.AuthApiService
{
    public class AuthApiService : IAuthApiService
    {
        private readonly AuthApiClient _authApiClient;
        private readonly ILogger<AuthApiService> _logger;

        public AuthApiService(AuthApiClient authApiClient, ILogger<AuthApiService> logger)
        {
            _authApiClient = authApiClient;
            _logger = logger;
        }

        public async Task<IEnumerable<UserInfo>> GetAllUsersAsync()
        {
            try
            {
                return await _authApiClient.UsersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all users from Auth API");
                throw;
            }
        }
        public async Task<UserInfo> GetUserById(Guid userId)
        {
            try
            {
                return await _authApiClient.GetUserByIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching user with ID {UserId} from Auth API", userId);
                throw;
            }
        }

        public async Task<UserInfo> MeAsync()
        {
            try
            {
                return await _authApiClient.MeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching current user from Auth API");
                throw;
            }
        }
    }
}
