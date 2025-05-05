
using UnitTestWriting.Domain;

namespace UnitTestWriting.Services
{
    public interface IUserRepository
    {
        Task<bool> ExistsAsync(Guid id);

        Task InsertAsync(User user);

        Task UpdateAsync(User user);
    }
}