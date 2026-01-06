using Server.Model;
using Server.Model.View;

namespace Server.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResult> RegisterAsync(RegisterRequest request);
        Task<string?> LoginAsync(LoginRequest request);
    }
}
