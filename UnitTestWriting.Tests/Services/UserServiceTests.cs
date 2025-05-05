using Moq;
using NUnit.Framework;
using UnitTestWriting.Domain;
using UnitTestWriting.Services;

namespace UnitTestWriting.Tests.Services;

public class UserServiceTests
{
    [Test]
    public async Task SaveAsync_ExistingUser_UpdatesUser()
    {
        // ARRANGE
        var utcNow = DateTime.UtcNow;
        var timeProvider = Mock.Of<TimeProvider>(p => p.GetUtcNow() == utcNow);
        var user = new User { Id = Guid.NewGuid() };
        var mockRepository = new Mock<IUserRepository>();
        mockRepository.Setup(rep => rep.ExistsAsync(user.Id)).ReturnsAsync(true);
        var userService = new UserService(mockRepository.Object, timeProvider);

        // ACT
        await userService.SaveAsync(user);

        // ASSERT
        mockRepository.Verify(rep => rep.UpdateAsync(It.Is<User>(u => u.UpdatedAt == utcNow)), times: Times.Once);
    }

    [Test]
    public async Task SaveAsync_NonExistingUser_InsertsUser()
    {
        // ARRANGE
        var utcNow = DateTime.UtcNow;
        var timeProvider = Mock.Of<TimeProvider>(p => p.GetUtcNow() == utcNow);
        var user = new User { Id = Guid.NewGuid() };
        var mockRepository = new Mock<IUserRepository>(MockBehavior.Strict);
        mockRepository.Setup(rep => rep.ExistsAsync(user.Id)).ReturnsAsync(false);
        mockRepository.Setup(rep => rep.InsertAsync(It.Is<User>(u => u.CreatedAt == utcNow)))
                      .Returns(Task.CompletedTask)
                      .Verifiable(Times.Once);
        var userService = new UserService(mockRepository.Object, timeProvider);

        // ACT
        await userService.SaveAsync(user);

        // ASSERT
        mockRepository.VerifyAll();
    }
}
