using CampusEats.Api.Data;
using CampusEats.Api.Infrastructure.Auth;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Auth.Refresh;

public class RefreshHandler(
    AppDbContext db,
    IJwtTokenService jwt,
    IHttpContextAccessor http
) : IRequestHandler<RefreshCommand, IResult>
{
    public async Task<IResult> Handle(RefreshCommand request, CancellationToken ct)
    {
        var ctx = http.HttpContext!;
        if (!ctx.Request.Cookies.TryGetValue("refresh_token", out var token) || string.IsNullOrWhiteSpace(token))
            return Results.Unauthorized();

        var hash = jwt.Hash(token);
        var rt = await db.RefreshTokens.FirstOrDefaultAsync(t =>
            t.TokenHash == hash && t.RevokedAtUtc == null && t.ExpiresAtUtc > DateTime.UtcNow, ct);

        if (rt is null)
            return Results.Unauthorized();
        
        var user = await db.Users.AsNoTracking().FirstAsync(u => u.Id == rt.UserId, ct);

        return Results.Ok(new { AccessToken = jwt.GenerateAccessToken(user) });
    }
}