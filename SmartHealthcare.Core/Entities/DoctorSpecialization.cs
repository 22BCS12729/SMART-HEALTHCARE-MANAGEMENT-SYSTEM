namespace SmartHealthcare.Core.Entities
{
    // Junction table for Many-to-Many relationship between Doctor and Specialization
    public class DoctorSpecialization : BaseEntity
    {
        public int DoctorId { get; set; }
        public virtual Doctor Doctor { get; set; } = null!;

        public int SpecializationId { get; set; }
        public virtual Specialization Specialization { get; set; } = null!;

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    }
}
