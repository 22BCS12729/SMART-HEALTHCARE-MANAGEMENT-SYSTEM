using Microsoft.AspNetCore.Mvc;
using SmartHealthcare.Core.DTOs;
using SmartHealthcare.MVC.Services;
using SmartHealthcare.MVC.ViewModels;

namespace SmartHealthcare.MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ApiService apiService, ILogger<AuthController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (_apiService.IsAuthenticated())
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var loginDto = new LoginDto
            {
                Email = model.Email,
                Password = model.Password
            };

            var result = await _apiService.LoginAsync(loginDto);

            if (result == null)
            {
                model.ErrorMessage = "Invalid email or password. Please try again.";
                return View(model);
            }

            // Store token in session
            _apiService.SetToken(result.Token);

            // Store user info in session
            HttpContext.Session.SetString("UserName", result.User.FullName);
            HttpContext.Session.SetString("UserRole", result.User.Role.ToString());
            HttpContext.Session.SetInt32("UserId", result.User.Id);

            _logger.LogInformation("User {Email} logged in successfully", model.Email);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterPatientViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterPatientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var createDto = new CreatePatientDto
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Password = model.Password,
                PhoneNumber = model.PhoneNumber,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                Address = model.Address,
                EmergencyContactName = model.EmergencyContactName,
                EmergencyContactPhone = model.EmergencyContactPhone,
                MedicalHistory = model.MedicalHistory,
                Allergies = model.Allergies,
                BloodGroup = model.BloodGroup
            };

            var result = await _apiService.PostAsync<PatientDto>("Auth/register/patient", createDto);

            if (result == null)
            {
                model.ErrorMessage = "Registration failed. Email may already be registered.";
                return View(model);
            }

            _logger.LogInformation("Patient {Email} registered successfully", model.Email);

            // Auto login after registration
            var loginResult = await _apiService.LoginAsync(new LoginDto
            {
                Email = model.Email,
                Password = model.Password
            });

            if (loginResult != null)
            {
                _apiService.SetToken(loginResult.Token);
                HttpContext.Session.SetString("UserName", loginResult.User.FullName);
                HttpContext.Session.SetString("UserRole", loginResult.User.Role.ToString());
                HttpContext.Session.SetInt32("UserId", loginResult.User.Id);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            _apiService.ClearToken();
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
