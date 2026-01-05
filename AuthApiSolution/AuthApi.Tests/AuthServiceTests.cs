using AuthApi.BusinessLogic.Services.Implementation;
using AuthApi.Contracts.Responses;
using AuthApi.Data.Repositories;
using AuthApi.Exceptions;
using NSubstitute;

namespace AuthApi.Tests;

public class AuthServiceTests
{
    private readonly AuthService _sut;
    private readonly IUserRepository _userRepository;

    public AuthServiceTests()
    {
        _userRepository = Substitute.For<IUserRepository>();

        _sut = new AuthService(
            _userRepository
        );
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
            new(Guid.NewGuid(), "test@example.com", "John", "Doe")
        };

        _userRepository.GetAllUsers().Returns(users);

        var result = await _sut.GetAllUserInfo();

        Assert.NotNull(result);
        Assert.Single(result);
    }
}