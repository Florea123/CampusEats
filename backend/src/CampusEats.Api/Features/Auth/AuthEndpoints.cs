using CampusEats.Api.Features.Auth.Login;
using CampusEats.Api.Features.Auth.Logout;
using CampusEats.Api.Features.Auth.Refresh;
using CampusEats.Api.Features.Auth.Register;
using MediatR;

namespace CampusEats.Api.Features.Auth;

public static class AuthEndpoints
{
    public static void MapAuth(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", async (LoginUserCommand cmd, IMediator mediator, CancellationToken ct) =>
            {
                var result = await mediator.Send(cmd, ct);
                return result;
            })
            .WithTags("Auth");
        
        app.MapPost("/auth/logout", async (IMediator mediator, CancellationToken ct) =>
            {
                var result = await mediator.Send(new LogoutCommand(), ct);
                return result;
            })
            .WithTags("Auth");
        
        app.MapPost("/auth/refresh", async (IMediator mediator, CancellationToken ct) =>
            {
                var result = await mediator.Send(new RefreshCommand(), ct);
                return result;
            })
            .WithTags("Auth");
        
        app.MapPost("/auth/register", async (RegisterUserCommand cmd, IMediator mediator, CancellationToken ct) =>
            {
                var result = await mediator.Send(cmd, ct);
                return result;
            })
            .WithTags("Auth");
    }
}