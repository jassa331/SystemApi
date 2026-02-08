using SystemApi.Entities;

namespace SystemApi.DAL.IRepository
{
    public interface IAuthRepository
    {
        Task RegisterUserAsync(User user);
        Task<User?> GetUserByEmailAsync(string email);
    }
}
