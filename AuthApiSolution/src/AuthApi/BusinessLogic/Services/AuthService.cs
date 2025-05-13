

using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using AuthApi.Contracts.Requests;
using AuthApi.Contracts.Responses;
using AuthApi.Data.Entities;
using AuthApi.Data.Repositories;
using AuthApi.Exceptions;
using Azure.Core;
using Microsoft.IdentityModel.Tokens;

namespace AuthApi.BusinessLogic.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuthService> _logger;
        private readonly Regex EmailValidatonRegex;
        private readonly string _jwtSecret;

        public AuthService(IUserRepository userRepository, ILogger<AuthService> logger, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _logger = logger;           
            _jwtSecret = configuration["JwtSettings:Secret"] ?? string.Empty;
            EmailValidatonRegex = new Regex("^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$");
        }

        public async Task<string> LoginAsync(LoginUserRequest loginUserDto)
        {
            var user = await _userRepository.GetByEmailAsync(loginUserDto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginUserDto.Password, user.PasswordHash))
            {
                throw new Exception("Invalid email or password");
            }

            return GenerateJwt(user);
        }

        public async Task<UserInfo> GetCurrentlyLoggedInUser(string userId)
        {
            if(!Guid.TryParse(userId, out Guid userIdGuid))
            {
                throw new ArgumentException("Incorrect argument");
            }
            var user = await _userRepository.GetByIdAsync(userIdGuid);

            if (user == null)
            {
                throw new NotFoundException("User not found");
            }
            return user;
        }

        public async Task<string> RegisterAsync(RegisterUserRequest registerUserDto)
        {
            if(!IsEmailValid(registerUserDto.Email))
            {
                throw new ArgumentException("Invalid email address");
            }

            var userExists = await _userRepository.CheckExistenceByEmail(registerUserDto.Email);

            if(userExists)
            {
                throw new ArgumentException("Email is already taken");
            }


            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerUserDto.Password);

            var user = new UserEntity
            {
                Email = registerUserDto.Email,
                PasswordHash = hashedPassword,
                Name = registerUserDto.Name,
                LastName = registerUserDto.LastName
            };

            await _userRepository.AddAsync(user);

            return GenerateJwt(user);
        }

        public async Task<List<UserInfo>> GetAllUserInfo()
        {
            return await _userRepository.GetAllUsers();
        }


        public async Task<bool> UserExists(string email)
        {
            return await _userRepository.CheckExistenceByEmail(email);
        }

        public async Task<UserInfo> GetUserById(Guid id)
        {
            return await _userRepository.GetByIdAsync(id);
        }


        private bool IsEmailValid(string email)
        {
           if(string.IsNullOrEmpty(email)) return false;

           if(EmailValidatonRegex.IsMatch(email)) return true;
            return false;
        }
        private string GenerateJwt(UserEntity user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "AuthApi",
                audience: "TaskApi",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
