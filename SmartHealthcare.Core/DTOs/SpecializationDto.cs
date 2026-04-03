using System.ComponentModel.DataAnnotations;

namespace SmartHealthcare.Core.DTOs
{
    // Specialization DTOs
    public class SpecializationDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DoctorCount { get; set; }
    }

    public class CreateSpecializationDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateSpecializationDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class SpecializationWithDoctorsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<DoctorListDto> Doctors { get; set; } = new();
    }

    public class AssignSpecializationDto
    {
        [Required]
        public int SpecializationId { get; set; }
    }
}
