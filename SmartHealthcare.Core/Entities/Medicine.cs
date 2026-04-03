using System.ComponentModel.DataAnnotations;

namespace SmartHealthcare.Core.Entities
{
    public class Medicine : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? GenericName { get; set; }

        [StringLength(200)]
        public string? Manufacturer { get; set; }

        [StringLength(100)]
        public string? DosageForm { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? SideEffects { get; set; }

        [StringLength(1000)]
        public string? Contraindications { get; set; }

        // Navigation property
        public virtual ICollection<PrescriptionMedicine> PrescriptionMedicines { get; set; } = new List<PrescriptionMedicine>();
    }
}
