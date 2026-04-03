using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartHealthcare.Core.Enums;

namespace SmartHealthcare.Core.Entities
{
    public class Patient : BaseEntity
    {
        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

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

        [StringLength(50)]
        public string? BloodGroup { get; set; }

        // Navigation properties
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
