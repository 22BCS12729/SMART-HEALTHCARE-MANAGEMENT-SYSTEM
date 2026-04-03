using System.ComponentModel.DataAnnotations;

namespace SmartHealthcare.Core.Entities
{
    public class Specialization : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation property for Many-to-Many relationship
        public virtual ICollection<DoctorSpecialization> DoctorSpecializations { get; set; } = new List<DoctorSpecialization>();
    }
}
