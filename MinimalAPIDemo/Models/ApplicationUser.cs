using Microsoft.AspNetCore.Identity;

namespace MinimalAPIDemo.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
    }
}
