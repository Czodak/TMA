using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthApi.BusinessLogic.Services;
using AuthApi.Contracts.Requests;
using AuthApi.Contracts.Responses;
using AuthApi.Data.Entities;
using AuthApi.Data.Repositories;
using AuthApi.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AuthApi.Tests
{
    public class AuthServiceTests
    {
        private readonly AuthService _sut;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _config;

        public AuthServiceTests()
        {
            _userRepository = Substitute.For<IUserRepository>();
            _logger = Substitute.For<ILogger<AuthService>>();
            _config = Substitute.For<IConfiguration>();

            _config["JwtSettings:Secret"].Returns("VerySecretTestKeyyyuyyjgv1234579869856!");

            _sut = new AuthService(
                _userRepository,
                _logger,
                _config
            );
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrow_WhenEmailInvalid()
        {
            var request = new RegisterUserRequest
            {
                Email = "invalid_email",
                Password = "test",
                Name = "John",
                LastName = "Doe"
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.RegisterAsync(request));
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrow_WhenEmailTaken()
        {
            var request = new RegisterUserRequest
            {
                Email = "test@example.com",
                Password = "test",
                Name = "John",
                LastName = "Doe"
            };

            _userRepository.CheckExistenceByEmail(request.Email).Returns(true);

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.RegisterAsync(request));
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnToken_WhenSuccess()
        {
            var request = new RegisterUserRequest
            {
                Email = "test@example.com",
                Password = "password123",
                Name = "John",
                LastName = "Doe"
            };

            _userRepository.CheckExistenceByEmail(request.Email).Returns(false);

            var token = await _sut.RegisterAsync(request);

            Assert.False(string.IsNullOrEmpty(token));
            await _userRepository.Received(1).AddAsync(Arg.Any<UserEntity>());
        }

        [Fact]
        public async Task LoginAsync_ShouldThrow_WhenUserNotFound()
        {
            var request = new LoginUserRequest
            {
                Email = "nonexistent@example.com",
                Password = "password123"
            };

            _userRepository.GetByEmailAsync(request.Email).Returns((UserEntity)null);

            await Assert.ThrowsAsync<Exception>(() => _sut.LoginAsync(request));
        }

        [Fact]
        public async Task LoginAsync_ShouldThrow_WhenPasswordInvalid()
        {
            var hashed = BCrypt.Net.BCrypt.HashPassword("correctpassword");
            var user = new UserEntity
            {
                Email = "test@example.com",
                PasswordHash = hashed
            };

            _userRepository.GetByEmailAsync(user.Email).Returns(user);

            var request = new LoginUserRequest
            {
                Email = user.Email,
                Password = "wrongpassword"
            };

            await Assert.ThrowsAsync<Exception>(() => _sut.LoginAsync(request));
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenSuccess()
        {
            var password = "password123";
            var hashed = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new UserEntity
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                PasswordHash = hashed
            };

            _userRepository.GetByEmailAsync(user.Email).Returns(user);

            var request = new LoginUserRequest
            {
                Email = user.Email,
                Password = password
            };

            var token = await _sut.LoginAsync(request);

            Assert.False(string.IsNullOrEmpty(token));
        }

        [Fact]
        public async Task GetCurrentlyLoggedInUser_ShouldThrow_WhenInvalidGuid()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetCurrentlyLoggedInUser("not-a-guid"));
        }

        [Fact]
        public async Task GetCurrentlyLoggedInUser_ShouldThrow_WhenUserNotFound()
        {
            var id = Guid.NewGuid().ToString();
            _userRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((UserInfo)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetCurrentlyLoggedInUser(id));
        }

        [Fact]
        public async Task GetCurrentlyLoggedInUser_ShouldReturnUserInfo_WhenUserFound()
        {
            var id = Guid.NewGuid();
            var userInfo = new UserInfo(id, "test@example.com", "John", "Doe");

            _userRepository.GetByIdAsync(id).Returns(userInfo);

            var result = await _sut.GetCurrentlyLoggedInUser(id.ToString());

            Assert.NotNull(result);
            Assert.Equal(userInfo.Id, result.Id);
            Assert.Equal(userInfo.Email, result.Email);
            Assert.Equal(userInfo.Name, result.Name);
            Assert.Equal(userInfo.LastName, result.LastName);
        }

        [Fact]
        public async Task GetUserById_ShouldReturnUserInfo_WhenUserFound()
        {
            var id = Guid.NewGuid();
            var userInfo = new UserInfo(id, "test@example.com", "Jane", "Smith");

            _userRepository.GetByIdAsync(id).Returns(userInfo);

            var result = await _sut.GetUserById(id);

            Assert.NotNull(result);
            Assert.Equal(userInfo.Id, result.Id);
            Assert.Equal(userInfo.Email, result.Email);
            Assert.Equal(userInfo.Name, result.Name);
            Assert.Equal(userInfo.LastName, result.LastName);
        }

        [Fact]
        public async Task UserExists_ShouldReturnTrue_WhenExists()
        {
            _userRepository.CheckExistenceByEmail("abc@example.com").Returns(true);

            var result = await _sut.UserExists("abc@example.com");

            Assert.True(result);
        }

        [Fact]
        public async Task UserExists_ShouldReturnFalse_WhenNotExists()
        {
            _userRepository.CheckExistenceByEmail("abc@example.com").Returns(false);

            var result = await _sut.UserExists("abc@example.com");

            Assert.False(result);
        }

        [Fact]
        public async Task GetAllUserInfo_ShouldReturnList()
        {
            var users = new List<UserInfo>
            {
                new UserInfo(Guid.NewGuid(), "test@example.com", "John", "Doe")
            };

            _userRepository.GetAllUsers().Returns(users);

            var result = await _sut.GetAllUserInfo();

            Assert.NotNull(result);
            Assert.Single(result);
        }
    }
}
