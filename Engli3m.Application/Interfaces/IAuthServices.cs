using Engli3m.Application.DTOs.Auth;

namespace Engli3m.Application.Interfaces
{
    public interface IAuthServices
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<bool> Logout(int userId);
        Task<bool> DeleteUserAsync(int userId);
    }

}
