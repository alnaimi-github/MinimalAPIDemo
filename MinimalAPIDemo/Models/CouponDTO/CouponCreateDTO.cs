namespace MinimalAPIDemo.Models.DTO
{
    public record CouponCreateDTO(
        string Name,
        int Percent,
        bool IsActive);
}
