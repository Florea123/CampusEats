using System.Security.Claims;
using CampusEats.Api.Data;
using CampusEats.Api.Enums;
using CampusEats.Api.Features.Orders.GetOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Orders.GetAllOrders;

public class GetAllOrdersHandler : IRequestHandler<GetAllOrdersQuery, List<OrderDto>>
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public GetAllOrdersHandler(AppDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public async Task<List<OrderDto>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var user = _http.HttpContext?.User;
        if (user == null) return new List<OrderDto>();

        var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(idClaim, out var userId)) return new List<OrderDto>();

        var isManager = user.IsInRole(UserRole.MANAGER.ToString())
                      || user.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == UserRole.MANAGER.ToString());

        var query = _db.Orders.AsNoTracking().Include(o => o.Items).AsQueryable();

        if (!isManager && !request.All)
            query = query.Where(o => o.UserId == userId);

        var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync(cancellationToken);

        var menuIds = orders.SelectMany(o => o.Items.Select(i => i.MenuItemId)).Distinct().ToList();
        var menuNames = await _db.MenuItems
            .AsNoTracking()
            .Where(mi => menuIds.Contains(mi.Id))
            .ToDictionaryAsync(mi => mi.Id, mi => mi.Name, cancellationToken);

        return orders.Select(o => new OrderDto
        {
            Id = o.Id,
            UserId = o.UserId,
            Status = o.Status,
            Total = o.Total,
            CreatedAtUtc = o.CreatedAt,
            UpdatedAtUtc = o.UpdatedAt,
            Notes = o.Notes,
            Items = o.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                MenuItemId = i.MenuItemId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                MenuItemName = menuNames.TryGetValue(i.MenuItemId, out var n) ? n : null
            }).ToList()
        }).ToList();
    }
}