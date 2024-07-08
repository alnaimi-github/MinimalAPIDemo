using FluentValidation;
using MinimalAPIDemo.Models.DTO;

namespace MinimalAPIDemo.Validations
{
    public class CouponCreateValidation:AbstractValidator<CouponCreateDTO>
    {
        public CouponCreateValidation()
        {
            
        }
    }
}
