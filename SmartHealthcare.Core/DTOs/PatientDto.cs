using System.ComponentModel.DataAnnotations;
using SmartHealthcare.Core.Enums;

namespace SmartHealthcare.Core.DTOs
{
    // Patient DTOs
    public class PatientDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? MedicalHistory { get; set; }
        public string? Allergies { get; set; }
        public string? BloodGroup { get; set; }
        public int Age => DateTime.Now.Year - DateOfBirth.Year;
    }

    public class CreatePatientDto
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
        public DateTime DateOfBirth { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? EmergencyContactName { get; set; }

        [Phone]
        [StringLength(20)]
        public string? EmergencyContactPhone { get; set; }

        [StringLength(1000)]
        public string? MedicalHistory { get; set; }

        [StringLength(1000)]
        public string? Allergies { get; set; }

        [StringLength(10)]
        public string? BloodGroup { get; set; }
    }

    public class UpdatePatientDto
    {
        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? EmergencyContactName { get; set; }

        [Phone]
        [StringLength(20)]
        public string? EmergencyContactPhone { get; set; }

        [StringLength(1000)]
        public string? MedicalHistory { get; set; }

        [StringLength(1000)]
        public string? Allergies { get; set; }

        [StringLength(10)]
        public string? BloodGroup { get; set; }
    }

    public class PatientProfileDto
    {
        public PatientDto Patient { get; set; } = null!;
        public List<AppointmentDto> UpcomingAppointments { get; set; } = new();
        public List<AppointmentDto> PastAppointments { get; set; } = new();
    }
}
