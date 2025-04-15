using Newtonsoft.Json;

namespace AuthApi.Contracts.Responses
{
    public class UserInfo
    {
        [JsonProperty]    
        public Guid Id { get; set; }
        
        [JsonProperty]
        public string Email { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string LastName { get; set; }

        public UserInfo(Guid id, string email, string name, string lastName)
        {
            Id = id;
            Email = email;
            Name = name;
            LastName = lastName;
        }
    }
}
