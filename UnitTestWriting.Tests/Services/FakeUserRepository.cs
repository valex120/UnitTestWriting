using UnitTestWriting.Domain;
using UnitTestWriting.Services;

namespace UnitTestWriting.Tests.Services;

public class FakeUserRepository : IUserRepository
{
    private bool _exists = false;

    public void SetupExists(bool exists)
    {
        _exists = exists;
    }

    public Task<bool> ExistsAsync(Guid id)
    {
        return Task.FromResult(_exists);
    }


    private bool _insertPerformed = false;

    public void VerifyInsert()
    {
        if (_insertPerformed is false)
        {
            throw new Exception("Update was not performed");
        }
    }

    public Task InsertAsync(User user)
    {
        _insertPerformed = true;
        return Task.CompletedTask;
    }

    private bool _updatePerformed = false;

    public void VerifyUpdate()
    {
        if (_updatePerformed is false)
        {
            throw new Exception("Update was not performed");
        }
    }

    public Task UpdateAsync(User user)
    {
        _updatePerformed = true;
        return Task.CompletedTask;
    }
}
