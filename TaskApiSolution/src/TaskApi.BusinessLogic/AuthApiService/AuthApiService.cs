using TaskApi.Common.HttpClients.Auth;

namespace TaskApi.BusinessLogic.AuthApiService
{
    public class AuthApiService : IAuthApiService
    {
        private readonly AuthApiClient _authApiClient;

        public AuthApiService(AuthApiClient authApiClient)
        {
            _authApiClient = authApiClient;
        }

        public async Task<IEnumerable<UserInfo>> GetAllUsersAsync()
        {
            return await _authApiClient.AllAsync();
        }

        public async Task<UserInfo> GetUserById(Guid userId)
        {
            return (await GetAllUsersAsync()).FirstOrDefault(x => x.Id == userId);
        }

        public async Task<UserInfo> MeAsync()
        {
            return await _authApiClient.MeAsync();
        }
    }
}
