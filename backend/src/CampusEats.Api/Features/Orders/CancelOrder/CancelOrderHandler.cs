using System.Security.Claims;
using CampusEats.Api.Data;
using CampusEats.Api.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Orders;

public class CancelOrderHandler : IRequestHandler<CancelOrderCommand, bool>
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;
    
    public CancelOrderHandler(AppDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public async Task<bool> Handle(CancelOrderCommand request, CancellationToken ct)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == request.Id, ct);
        if (order == null) return false;

        var user = _http.HttpContext?.User ?? throw new InvalidOperationException("No authenticated user.");
        var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(idClaim, out var userId)) return false;

        var isManager = user.IsInRole(UserRole.MANAGER.ToString())
                        || user.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == UserRole.MANAGER.ToString());

        // disallow cancelling if already cancelled or completed
        if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Completed)
            return false;

        // owner or manager can cancel
        if (!isManager && order.UserId != userId) return false;

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        _db.Orders.Update(order);
        var affected = await _db.SaveChangesAsync(ct);
        return affected > 0;
    }
}
