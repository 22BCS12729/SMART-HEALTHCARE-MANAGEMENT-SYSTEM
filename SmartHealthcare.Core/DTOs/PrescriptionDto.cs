using System.ComponentModel.DataAnnotations;

namespace SmartHealthcare.Core.DTOs
{
    // Prescription DTOs
    public class PrescriptionDto
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public DateTime PrescriptionDate { get; set; }
        public string? Diagnosis { get; set; }
        public string? Advice { get; set; }
        public string? FollowUpInstructions { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public List<PrescriptionMedicineDto> Medicines { get; set; } = new();
    }

    public class CreatePrescriptionDto
    {
        [Required]
        public int AppointmentId { get; set; }

        [StringLength(2000)]
        public string? Diagnosis { get; set; }

        [StringLength(2000)]
        public string? Advice { get; set; }

        [StringLength(1000)]
        public string? FollowUpInstructions { get; set; }

        public DateTime? FollowUpDate { get; set; }

        public List<CreatePrescriptionMedicineDto> Medicines { get; set; } = new();
    }

    public class UpdatePrescriptionDto
    {
        [StringLength(2000)]
        public string? Diagnosis { get; set; }

        [StringLength(2000)]
        public string? Advice { get; set; }

        [StringLength(1000)]
        public string? FollowUpInstructions { get; set; }

        public DateTime? FollowUpDate { get; set; }
    }

    public class PrescriptionMedicineDto
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string? Instructions { get; set; }
        public int Quantity { get; set; }
    }

    public class CreatePrescriptionMedicineDto
    {
        [Required]
        public int MedicineId { get; set; }

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

    public class PrescriptionListDto
    {
        public int Id { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public DateTime PrescriptionDate { get; set; }
        public string? Diagnosis { get; set; }
        public int MedicineCount { get; set; }
    }
}
