using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartHealthcare.Core.DTOs;
using SmartHealthcare.Core.Enums;
using SmartHealthcare.MVC.Services;
using SmartHealthcare.MVC.ViewModels;

namespace SmartHealthcare.MVC.Controllers
{
    [Route("[controller]")]
    public class AppointmentsController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(ApiService apiService, ILogger<AppointmentsController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            if (!_apiService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth");
            }

            var userRole = HttpContext.Session.GetString("UserRole");
            List<AppointmentDto> appointments = new();

            if (userRole == "Doctor")
            {
                var result = await _apiService.GetAsync<List<AppointmentDto>>("Appointments/my-appointments");
                if (result != null)
                {
                    appointments = result;
                }
            }
            else if (userRole == "Patient")
            {
                var result = await _apiService.GetAsync<List<AppointmentDto>>("Appointments/my-appointments");
                if (result != null)
                {
                    appointments = result;
                }
            }

            return View(appointments);
        }

        [HttpGet]
        [Route("Book")]
        public async Task<IActionResult> Book()
        {
            if (!_apiService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth");
            }

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Patient")
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var model = new AppointmentBookingViewModel();

            // Load available doctors
            var doctors = await _apiService.GetAsync<PagedResultDto<DoctorListDto>>("Doctors?PageNumber=1&PageSize=100");
            if (doctors != null)
            {
                model.AvailableDoctors = doctors.Items.ToList();
            }

            return View(model);
        }

        [HttpPost]
        [Route("Book")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(AppointmentBookingViewModel model)
        {
            if (!_apiService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                // Reload doctors on validation error
                var doctors = await _apiService.GetAsync<PagedResultDto<DoctorListDto>>("Doctors?PageNumber=1&PageSize=100");
                if (doctors != null)
                {
                    model.AvailableDoctors = doctors.Items.ToList();
                }
                return View(model);
            }

            var bookDto = new BookAppointmentRequestDto
            {
                DoctorId = model.DoctorId,
                AppointmentDate = model.AppointmentDate,
                AppointmentTime = model.AppointmentTime,
                Reason = model.Reason
            };

            var result = await _apiService.PostAsync<AppointmentDto>("Appointments/book", bookDto);

            if (result == null)
            {
                model.ErrorMessage = "Failed to book appointment. The time slot may no longer be available.";
                // Reload doctors
                var doctors = await _apiService.GetAsync<PagedResultDto<DoctorListDto>>("Doctors?PageNumber=1&PageSize=100");
                if (doctors != null)
                {
                    model.AvailableDoctors = doctors.Items.ToList();
                }
                return View(model);
            }

            _logger.LogInformation("Appointment booked successfully for doctor {DoctorId}", model.DoctorId);
            TempData["SuccessMessage"] = "Appointment booked successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("GetAvailableSlots")]
        public async Task<IActionResult> GetAvailableSlots(int doctorId, DateTime date)
        {
            if (!_apiService.IsAuthenticated())
            {
                return Json(new List<AppointmentSlotDto>());
            }

            var slots = await _apiService.GetAsync<List<AppointmentSlotDto>>($"Doctors/{doctorId}/available-slots?date={date:yyyy-MM-dd}");
            return Json(slots ?? new List<AppointmentSlotDto>());
        }

        [HttpPost]
        [Route("Cancel/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? reason)
        {
            if (!_apiService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth");
            }

            var result = await _apiService.PostAsync<object>($"Appointments/{id}/cancel", reason ?? "");

            if (result == null)
            {
                TempData["ErrorMessage"] = "Failed to cancel appointment.";
            }
            else
            {
                TempData["SuccessMessage"] = "Appointment cancelled successfully.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("UpdateStatus/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, AppointmentStatus status, string? notes)
        {
            if (!_apiService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth");
            }

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Doctor" && userRole != "Admin")
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var updateDto = new UpdateAppointmentStatusDto
            {
                Status = status,
                Notes = notes
            };

            var result = await _apiService.PatchAsync<AppointmentDto>($"Appointments/{id}/status", updateDto);

            if (result == null)
            {
                TempData["ErrorMessage"] = "Failed to update appointment status.";
            }
            else
            {
                TempData["SuccessMessage"] = $"Appointment status updated to {status}.";
            }

            return RedirectToAction("Index");
        }
    }
}
