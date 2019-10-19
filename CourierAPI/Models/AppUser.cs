using Microsoft.AspNetCore.Identity;

namespace CourierAPI.Models
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; }
    }
}