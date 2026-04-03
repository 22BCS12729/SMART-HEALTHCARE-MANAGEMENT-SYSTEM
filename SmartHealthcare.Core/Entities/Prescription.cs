using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartHealthcare.Core.Entities
{
    public class Prescription : BaseEntity
    {
        [Required]
        public int AppointmentId { get; set; }

        [ForeignKey("AppointmentId")]
        public virtual Appointment Appointment { get; set; } = null!;

        [Required]
        public int DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; } = null!;

        [Required]
        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; } = null!;

        [Required]
        public DateTime PrescriptionDate { get; set; } = DateTime.UtcNow;

        [StringLength(2000)]
        public string? Diagnosis { get; set; }

        [StringLength(2000)]
        public string? Advice { get; set; }

        [StringLength(1000)]
        public string? FollowUpInstructions { get; set; }

        public DateTime? FollowUpDate { get; set; }

        // Navigation property for Medicines
        public virtual ICollection<PrescriptionMedicine> PrescriptionMedicines { get; set; } = new List<PrescriptionMedicine>();
    }
}
