namespace MinimalAPIDemo.Models.DTO
{
    public record CouponUpdateDTO(
        int Id,
        string Name,
        int Percent,
        bool IsActive);
}
