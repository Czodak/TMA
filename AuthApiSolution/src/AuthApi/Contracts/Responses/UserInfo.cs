using Newtonsoft.Json;

namespace AuthApi.Contracts.Responses
{
    public class UserInfo
    {
        [JsonProperty]    
        public Guid Id { get; set; }
        [JsonProperty]
        public string Email { get; set; }

        public UserInfo(Guid id, string email)
        {
            Id = id;
            Email = email;
        }
    }
}
