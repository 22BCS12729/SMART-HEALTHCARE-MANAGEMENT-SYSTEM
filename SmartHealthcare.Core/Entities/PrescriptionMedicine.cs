using System.ComponentModel.DataAnnotations;

namespace SmartHealthcare.Core.Entities
{
    // Junction table for Many-to-Many relationship between Prescription and Medicine
    public class PrescriptionMedicine : BaseEntity
    {
        public int PrescriptionId { get; set; }
        public virtual Prescription Prescription { get; set; } = null!;

        public int MedicineId { get; set; }
        public virtual Medicine Medicine { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Dosage { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Frequency { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Duration { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Instructions { get; set; }

        public int Quantity { get; set; }
    }
}
