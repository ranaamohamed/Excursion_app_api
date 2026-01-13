using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common_Authentication.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool? sendOffers { get; set; }
        public int completeprofile { get; set; }
        public string? GoogleId { get; set; }
        public string? RefreshToken { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
