using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartHealthcare.Core.Enums;

namespace SmartHealthcare.Core.Entities
{
    public class Doctor : BaseEntity
    {
        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

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

        // Navigation properties
        // One-to-Many: One Doctor has Many Appointments
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        // Many-to-Many: Doctor <-> Specializations
        public virtual ICollection<DoctorSpecialization> DoctorSpecializations { get; set; } = new List<DoctorSpecialization>();

        // Navigation properties for Prescriptions
        public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}
