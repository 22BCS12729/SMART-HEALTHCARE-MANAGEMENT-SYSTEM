using System.ComponentModel.DataAnnotations;
using SmartHealthcare.Core.DTOs;
using SmartHealthcare.Core.Enums;

namespace SmartHealthcare.MVC.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class RegisterPatientViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [Display(Name = "Gender")]
        public Gender Gender { get; set; }

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [StringLength(50, ErrorMessage = "Emergency contact name cannot exceed 50 characters")]
        [Display(Name = "Emergency Contact Name")]
        public string? EmergencyContactName { get; set; }

        [Phone(ErrorMessage = "Invalid emergency contact phone")]
        [StringLength(20, ErrorMessage = "Emergency contact phone cannot exceed 20 characters")]
        [Display(Name = "Emergency Contact Phone")]
        public string? EmergencyContactPhone { get; set; }

        [StringLength(1000, ErrorMessage = "Medical history cannot exceed 1000 characters")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Medical History")]
        public string? MedicalHistory { get; set; }

        [StringLength(1000, ErrorMessage = "Allergies cannot exceed 1000 characters")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Allergies")]
        public string? Allergies { get; set; }

        [StringLength(10, ErrorMessage = "Blood group cannot exceed 10 characters")]
        [Display(Name = "Blood Group")]
        public string? BloodGroup { get; set; }

        public string? ErrorMessage { get; set; }
    }

    public class DashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public DashboardStatsDto? Stats { get; set; }
        public List<AppointmentDto> UpcomingAppointments { get; set; } = new();
        public List<DoctorListDto> Doctors { get; set; } = new();
    }

    public class AppointmentBookingViewModel
    {
        [Required(ErrorMessage = "Please select a doctor")]
        [Display(Name = "Doctor")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Appointment date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Appointment Date")]
        public DateTime AppointmentDate { get; set; } = DateTime.Now.AddDays(1);

        [Required(ErrorMessage = "Appointment time is required")]
        [DataType(DataType.Time)]
        [Display(Name = "Appointment Time")]
        public TimeSpan AppointmentTime { get; set; } = new TimeSpan(9, 0, 0);

        [StringLength(1000, ErrorMessage = "Reason cannot exceed 1000 characters")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Reason for Visit")]
        public string? Reason { get; set; }

        public List<DoctorListDto> AvailableDoctors { get; set; } = new();
        public List<AppointmentSlotDto> AvailableSlots { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm New Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }
}
