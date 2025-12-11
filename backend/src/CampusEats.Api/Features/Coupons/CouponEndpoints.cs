using CampusEats.Api.Features.Coupons.CreateCoupon;
using CampusEats.Api.Features.Coupons.GetAvailableCoupons;
using CampusEats.Api.Features.Coupons.GetUserCoupons;
using CampusEats.Api.Features.Coupons.PurchaseCoupon;

namespace CampusEats.Api.Features.Coupons;

public static class CouponEndpoints
{
    public static void MapCouponEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCreateCoupon();
        app.MapGetAvailableCoupons();
        app.MapGetUserCoupons();
        app.MapPurchaseCoupon();
    }
}
