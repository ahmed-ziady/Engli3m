using Engli3m.Domain.Enities;

namespace Engli3m.Application.Interfaces
{
    // Engli3m.Application/Interfaces/ITokenService.cs

    public interface ITokenService
        {
            string GenerateJwtToken(User user, IList<string> roles);
        }
    
}
