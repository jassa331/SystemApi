using SystemApi.DTO;

namespace SystemApi.BLL.Interface
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterRequestDto dto);
        Task<string> LoginAsync(loginDto dto);
    }
}
