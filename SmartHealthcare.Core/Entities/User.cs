using System.ComponentModel.DataAnnotations;
using SmartHealthcare.Core.Enums;

namespace SmartHealthcare.Core.Entities
{
    public class User : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        public UserRole Role { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public string? RefreshToken { get; set; }
        
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Navigation property for One-to-One relationship with Patient
        public virtual Patient? Patient { get; set; }

        // Navigation property for One-to-One relationship with Doctor
        public virtual Doctor? Doctor { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}
