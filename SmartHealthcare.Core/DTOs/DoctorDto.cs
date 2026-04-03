using System.ComponentModel.DataAnnotations;

namespace SmartHealthcare.Core.DTOs
{
    // Doctor DTOs
    public class DoctorDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public string Qualifications { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public string? Biography { get; set; }
        public string? ConsultationFee { get; set; }
        public string? AvailableDays { get; set; }
        public TimeSpan? AvailableFrom { get; set; }
        public TimeSpan? AvailableTo { get; set; }
        public string? Address { get; set; }
        public List<SpecializationDto> Specializations { get; set; } = new();
        public string FullName => $"{FirstName} {LastName}";
    }

    public class DoctorListDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public string? ConsultationFee { get; set; }
        public List<string> SpecializationNames { get; set; } = new();
        public string FullName => $"{FirstName} {LastName}";
    }

    public class CreateDoctorDto
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
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string LicenseNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Qualifications { get; set; } = string.Empty;

        [Required]
        [Range(0, 50)]
        public int ExperienceYears { get; set; }

        [StringLength(1000)]
        public string? Biography { get; set; }

        [StringLength(500)]
        public string? ConsultationFee { get; set; }

        [StringLength(200)]
        public string? AvailableDays { get; set; }

        public TimeSpan? AvailableFrom { get; set; }
        public TimeSpan? AvailableTo { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        public List<int> SpecializationIds { get; set; } = new();
    }

    public class UpdateDoctorDto
    {
        [StringLength(500)]
        public string? Qualifications { get; set; }

        [Range(0, 50)]
        public int? ExperienceYears { get; set; }

        [StringLength(1000)]
        public string? Biography { get; set; }

        [StringLength(500)]
        public string? ConsultationFee { get; set; }

        [StringLength(200)]
        public string? AvailableDays { get; set; }

        public TimeSpan? AvailableFrom { get; set; }
        public TimeSpan? AvailableTo { get; set; }

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        public List<int>? SpecializationIds { get; set; }
    }

    public class DoctorScheduleDto
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string? AvailableDays { get; set; }
        public TimeSpan? AvailableFrom { get; set; }
        public TimeSpan? AvailableTo { get; set; }
        public List<AppointmentSlotDto> BookedSlots { get; set; } = new();
    }

    public class AppointmentSlotDto
    {
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public bool IsAvailable { get; set; }
    }
}
