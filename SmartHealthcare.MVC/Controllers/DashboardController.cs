using Microsoft.AspNetCore.Mvc;
using SmartHealthcare.Core.DTOs;
using SmartHealthcare.MVC.Services;
using SmartHealthcare.MVC.ViewModels;

namespace SmartHealthcare.MVC.Controllers
{
    [Route("[controller]")]
    public class DashboardController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ApiService apiService, ILogger<DashboardController> logger)
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
            var userName = HttpContext.Session.GetString("UserName");

            var model = new DashboardViewModel
            {
                UserName = userName ?? "User",
                UserRole = userRole ?? "Unknown"
            };

            // Load role-specific data
            if (userRole == "Admin")
            {
                // Admin sees system-wide stats
                var stats = await _apiService.GetAsync<DashboardStatsDto>("Appointments/stats");
                model.Stats = stats;
            }
            else if (userRole == "Doctor")
            {
                // Doctor sees today's appointments and stats
                var appointments = await _apiService.GetAsync<List<AppointmentDto>>("Appointments/today");
                if (appointments != null)
                {
                    model.UpcomingAppointments = appointments;
                }
            }
            else if (userRole == "Patient")
            {
                // Patient sees their upcoming appointments
                var profile = await _apiService.GetAsync<PatientProfileDto>("Patients/profile");
                if (profile != null)
                {
                    model.UpcomingAppointments = profile.UpcomingAppointments;
                }
            }

            return View(model);
        }
    }
}
