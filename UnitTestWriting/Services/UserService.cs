using UnitTestWriting.Domain;

namespace UnitTestWriting.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly TimeProvider _timeProvider;

        public UserService(IUserRepository userRepository, TimeProvider timeProvider)
        {
            _userRepository = userRepository;
            _timeProvider = timeProvider;
        }

        public async Task SaveAsync(User user)
        {
            var exists = await _userRepository.ExistsAsync(user.Id);

            if (exists)
            {
                user.UpdatedAt = _timeProvider.GetUtcNow();
                await _userRepository.UpdateAsync(user);
            }
            else
            {
                user.CreatedAt = _timeProvider.GetUtcNow();
                await _userRepository.InsertAsync(user);
            }
        }
    }
}
