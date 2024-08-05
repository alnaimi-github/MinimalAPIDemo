using System.ComponentModel.DataAnnotations;

namespace MinimalAPIDemo.Models;

public class RefreshToken
{
    [Key] public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string JwtTokenId { get; set; } = string.Empty;
    public string RefreshJwtToken { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public DateTime ExpireAt { get; set; }
}