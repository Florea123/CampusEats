using CampusEats.Api.Data;
using CampusEats.Api.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Menu.UpdateMenuItem;

public class UpdateMenuItemHandler(AppDbContext db) : IRequestHandler<UpdateMenuItemCommand, bool>
{
    public async Task<bool> Handle(UpdateMenuItemCommand request, CancellationToken ct)
    {
        var entity = await db.MenuItems.FindAsync([request.Id], ct);
        if (entity is null) return false;

        var updated = entity with
        {
            Name = request.Name.Trim(),
            Price = request.Price,
            Description = request.Description?.Trim(),
            Category = request.Category,
            ImageUrl = request.ImageUrl?.Trim(),
            Allergens = request.Allergens ?? []
        };

        db.Entry(entity).CurrentValues.SetValues(updated);
        await db.SaveChangesAsync(ct);
        return true;
    }
}