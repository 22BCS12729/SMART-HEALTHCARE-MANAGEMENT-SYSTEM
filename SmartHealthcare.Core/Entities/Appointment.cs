using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartHealthcare.Core.Enums;

namespace SmartHealthcare.Core.Entities
{
    public class Appointment : BaseEntity
    {
        [Required]
        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; } = null!;

        [Required]
        public int DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; } = null!;

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan AppointmentTime { get; set; }

        [Required]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        [StringLength(1000)]
        public string? Reason { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public int? DurationMinutes { get; set; } = 30;

        // Navigation property
        public virtual Prescription? Prescription { get; set; }
    }
}
