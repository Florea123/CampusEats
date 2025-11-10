
using CampusEats.Api.Data;
using MediatR;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using CampusEats.Api.Domain;
using CampusEats.Api.Features.Orders;
using CampusEats.Api.Enums;
namespace CampusEats.Api.Features.Orders.GetOrders;

public class GetOrdersHandler :
    IRequestHandler<GetAllOrdersQuery, List<OrderDto>>,
    IRequestHandler<GetOrdersQuerry, OrderDto?>
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public GetOrdersHandler(AppDbContext db, IHttpContextAccessor http)
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

        if (!isManager) 
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

    public async Task<OrderDto?> Handle(GetOrdersQuerry request, CancellationToken cancellationToken)
    {
        var user = _http.HttpContext?.User;
        if (user == null) return null;

        var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(idClaim, out var userId)) return null;

        var isAdmin = user.IsInRole(UserRole.MANAGER.ToString())
                      || user.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == UserRole.MANAGER.ToString());

        var order = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (order == null) return null;
        if (!isAdmin && order.UserId != userId) return null;

        var menuIds = order.Items.Select(i => i.MenuItemId).Distinct().ToList();
        var menuNames = await _db.MenuItems
            .AsNoTracking()
            .Where(mi => menuIds.Contains(mi.Id))
            .ToDictionaryAsync(mi => mi.Id, mi => mi.Name, cancellationToken);

        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            Status = order.Status,
            Total = order.Total,
            CreatedAtUtc = order.CreatedAt,
            UpdatedAtUtc = order.UpdatedAt,
            Notes = order.Notes,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                MenuItemId = i.MenuItemId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                MenuItemName = menuNames.TryGetValue(i.MenuItemId, out var n) ? n : null
            }).ToList()
        };
    }
}