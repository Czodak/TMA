using System.ComponentModel.DataAnnotations;

namespace AuthApi.Contracts.Requests
{
    public class LoginUserRequest
    {
        [Required]
        [MaxLength(255)]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
