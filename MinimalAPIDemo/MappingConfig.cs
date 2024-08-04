using AutoMapper;
using MinimalAPIDemo.Models;
using MinimalAPIDemo.Models.DTO;

namespace MinimalAPIDemo
{
    public class MappingConfig:Profile
    {
        public MappingConfig()
        {
            //Coupon
            CreateMap<Coupon, CouponCreateDTO>().ReverseMap();
            CreateMap<Coupon, CouponDTO>().ReverseMap();
            CreateMap<Coupon, CouponUpdateDTO>().ReverseMap();
        }
    }
}
