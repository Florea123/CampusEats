namespace CampusEats.Api.Features.Loyalty.RedeemPoints;

public record RedeemPointsResult(bool Success, string Message, int? RemainingPoints = null);