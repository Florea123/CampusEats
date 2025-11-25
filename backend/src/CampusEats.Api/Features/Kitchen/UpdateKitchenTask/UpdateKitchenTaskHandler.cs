using CampusEats.Api.Data;
using CampusEats.Api.Domain;
using CampusEats.Api.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Kitchen.UpdateKitchenTask;

public class UpdateKitchenTaskHandler(AppDbContext db)
    : IRequestHandler<UpdateKitchenTaskCommand, IResult>
{
    public async Task<IResult> Handle(UpdateKitchenTaskCommand request, CancellationToken ct)
    {
        // 1. Găsim Task-ul de bucătărie
        var kitchenTask = await db.KitchenTasks
            .FirstOrDefaultAsync(t => t.Id == request.Id, ct);

        if (kitchenTask is null)
            return Results.NotFound($"Kitchen task {request.Id} not found.");

        // 2. Actualizăm câmpurile opționale
        if (request.AssignedTo is not null && request.AssignedTo != Guid.Empty)
            kitchenTask.AssignedTo = request.AssignedTo.Value;

        if (!string.IsNullOrWhiteSpace(request.Notes))
            kitchenTask.Notes = request.Notes.Trim();

        // 3. Actualizăm statusul și sincronizăm cu Comanda
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Enum.TryParse<KitchenTaskStatus>(request.Status, true, out var newStatus))
                return Results.BadRequest("Invalid status value.");

            kitchenTask.Status = newStatus;

            // --- FIX: Actualizare Status Comanda ---
            // Căutăm comanda asociată acestui task
            var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == kitchenTask.OrderId, ct);
            
            if (order != null)
            {
                // Mapăm statusul din bucătărie la statusul comenzii
                switch (newStatus)
                {
                    case KitchenTaskStatus.Preparing:
                        order.Status = OrderStatus.Preparing;
                        break;
                    
                    // Dacă e gata sau completată în bucătărie -> Completed pentru student
                    case KitchenTaskStatus.Ready:
                    case KitchenTaskStatus.Completed:
                        order.Status = OrderStatus.Completed;
                        break;
                    
                    // Putem trata și alte cazuri dacă e nevoie
                }
                
                // Actualizăm și timestamp-ul comenzii
                order.UpdatedAt = DateTime.UtcNow;
            }
            // ---------------------------------------
        }

        kitchenTask.UpdatedAt = DateTime.UtcNow;

        // Salvăm ambele modificări (KitchenTask și Order) în aceeași tranzacție
        await db.SaveChangesAsync(ct);

        return Results.Ok(kitchenTask);
    }
}