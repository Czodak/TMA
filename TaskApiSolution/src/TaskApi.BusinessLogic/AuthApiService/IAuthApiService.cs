using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskApi.Common.HttpClients.Auth;

namespace TaskApi.BusinessLogic.AuthApiService
{
    public interface IAuthApiService
    {
        Task<IEnumerable<UserInfo>> GetAllUsersAsync();
        Task<UserInfo> GetUserById(Guid userId);

        Task<UserInfo> MeAsync();
    }
}
