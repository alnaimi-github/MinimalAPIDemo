using Microsoft.EntityFrameworkCore;
using MinimalAPIDemo.Data;
using MinimalAPIDemo.Models;
using MinimalAPIDemo.Repository.IRepository;

namespace MinimalAPIDemo.Repository
{
    public class CouponRepository(ApplicationDbContext db): ICouponRepository
    {
        public async Task<ICollection<Coupon>> GetAllAsync()
        {
            return await db.Coupons.ToListAsync();
        }

        public async Task<Coupon> GetAsync(int id)
        {
            var result = ReturnCouponById(id);
            return result??null!;
        }

        public async Task CreateAsync(Coupon coupon)
        {
           await db.AddAsync(coupon);
        }

        public async Task UpdateAsync(Coupon coupon)
        {
            db.Update(coupon);
        }

        public async Task DeleteAsync(Coupon coupon)
        {
            db.Remove(coupon);
        }

        public async Task SaveAsync()
        {
          await  db.SaveChangesAsync();
        }

        public async Task<Coupon> GetAsync(string couponName)
        {
            var result=await db.Coupons.FirstOrDefaultAsync(x => x.Name.ToLower()==couponName.ToLower())!;
            return result??null!;
        }

        private Coupon ReturnCouponById(int id)
        {
            return db.Coupons.FirstOrDefault(x => x.Id == id)!;
        }
    }
}
