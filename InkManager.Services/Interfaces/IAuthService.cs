using InkManager.Core.DTOs;

namespace InkManager.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginDto dto);
        Task<SessionInfoDto> SelectRolAndEstudioAsync(SelectRolEstudioDto dto);
        string GenerateJwtToken(SessionInfoDto session);
    }
}