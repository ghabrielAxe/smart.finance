using System.Security.Claims;
using SmartFinance.Application.Interfaces;

namespace SmartFinance.Api.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserService
{
    public Guid UserId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user == null)
                return Guid.Empty;

            var idClaim =
                user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");

            return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
        }
    }

    public bool IsAuthenticated => UserId != Guid.Empty;
}
