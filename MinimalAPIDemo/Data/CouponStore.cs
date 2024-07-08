using MinimalAPIDemo.Models;

namespace MinimalAPIDemo.Data
{
    public static class CouponStore
    {
        public static List<Coupon> CouponsList =
            [
            new Coupon { Id=1,Name="10OF",Percent=10,IsActive=true},
            new Coupon { Id=2,Name="20OF",Percent=20,IsActive=false}
            ];
    }
}
