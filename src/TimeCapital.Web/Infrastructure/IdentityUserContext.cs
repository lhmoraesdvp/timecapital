using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace TimeCapital.Web.Infrastructure;

public sealed class IdentityUserContext : IUserContext
{
    private readonly IHttpContextAccessor _http;

    public IdentityUserContext(IHttpContextAccessor http)
    {
        _http = http;
    }

    public string GetUserId()
    {
        var id = _http.HttpContext?.User?
            .FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(id))
            throw new InvalidOperationException("Usuário não autenticado.");

        return id;
    }
}