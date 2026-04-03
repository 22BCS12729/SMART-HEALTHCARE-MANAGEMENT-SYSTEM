using System.ComponentModel.DataAnnotations;
using SmartHealthcare.Core.Enums;

namespace SmartHealthcare.Core.DTOs
{
    // Appointment DTOs
    public class AppointmentDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public int? DurationMinutes { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool HasPrescription => PrescriptionId.HasValue;
        public int? PrescriptionId { get; set; }
    }

    public class CreateAppointmentDto
    {
        [Required]
        public int DoctorId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan AppointmentTime { get; set; }

        [StringLength(1000)]
        public string? Reason { get; set; }

        public int? DurationMinutes { get; set; } = 30;
    }

    public class UpdateAppointmentDto
    {
        public DateTime? AppointmentDate { get; set; }
        public TimeSpan? AppointmentTime { get; set; }
        public AppointmentStatus? Status { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    public class AppointmentFilterDto
    {
        public int? PatientId { get; set; }
        public int? DoctorId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public AppointmentStatus? Status { get; set; }
    }

    public class AppointmentStatsDto
    {
        public int TotalAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int ConfirmedAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int TodayAppointments { get; set; }
    }

    public class BookAppointmentRequestDto
    {
        [Required]
        public int DoctorId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan AppointmentTime { get; set; }

        [StringLength(1000)]
        public string? Reason { get; set; }
    }

    public class UpdateAppointmentStatusDto
    {
        [Required]
        public AppointmentStatus Status { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}
