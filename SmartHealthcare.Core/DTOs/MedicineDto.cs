using System.ComponentModel.DataAnnotations;

namespace SmartHealthcare.Core.DTOs
{
    // Medicine DTOs
    public class MedicineDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? GenericName { get; set; }
        public string? Manufacturer { get; set; }
        public string? DosageForm { get; set; }
        public string? Description { get; set; }
        public string? SideEffects { get; set; }
        public string? Contraindications { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateMedicineDto
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
    }

    public class UpdateMedicineDto
    {
        [StringLength(200)]
        public string? Name { get; set; }

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

        public bool? IsActive { get; set; }
    }

    public class MedicineListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? GenericName { get; set; }
        public string? Manufacturer { get; set; }
        public string? DosageForm { get; set; }
    }

    public class MedicineSearchDto
    {
        public string? SearchTerm { get; set; }
        public string? Manufacturer { get; set; }
        public string? DosageForm { get; set; }
    }
}
