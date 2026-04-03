using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Core.Data;
using SmartHealthcare.Core.Entities;
using SmartHealthcare.Core.Enums;
using SmartHealthcare.API.Helpers;

namespace SmartHealthcare.API.Services
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Seed Specializations
            if (!context.Specializations.Any())
            {
                var specializations = new List<Specialization>
                {
                    new() { Name = "General Medicine", Description = "General health checkups and common illnesses" },
                    new() { Name = "Cardiology", Description = "Heart and cardiovascular system" },
                    new() { Name = "Dermatology", Description = "Skin, hair, and nail conditions" },
                    new() { Name = "Pediatrics", Description = "Children's health and development" },
                    new() { Name = "Orthopedics", Description = "Bones, joints, and muscles" },
                    new() { Name = "Neurology", Description = "Brain and nervous system" },
                    new() { Name = "Gynecology", Description = "Women's reproductive health" },
                    new() { Name = "Ophthalmology", Description = "Eye care and vision" }
                };
                await context.Specializations.AddRangeAsync(specializations);
                await context.SaveChangesAsync();
            }

            // Seed Medicines
            if (!context.Medicines.Any())
            {
                var medicines = new List<Medicine>
                {
                    new() { Name = "Paracetamol", Description = "Pain reliever and fever reducer", Manufacturer = "PharmaCo", DosageForm = "Tablet" },
                    new() { Name = "Amoxicillin", Description = "Antibiotic for bacterial infections", Manufacturer = "MediLabs", DosageForm = "Capsule" },
                    new() { Name = "Ibuprofen", Description = "Anti-inflammatory pain reliever", Manufacturer = "HealthCorp", DosageForm = "Tablet" },
                    new() { Name = "Cetirizine", Description = "Antihistamine for allergies", Manufacturer = "AllergyCare", DosageForm = "Tablet" },
                    new() { Name = "Metformin", Description = "Diabetes medication", Manufacturer = "DiabetesCare", DosageForm = "Tablet" },
                    new() { Name = "Omeprazole", Description = "Acid reflux treatment", Manufacturer = "GastroPharm", DosageForm = "Capsule" },
                    new() { Name = "Aspirin", Description = "Blood thinner and pain reliever", Manufacturer = "HeartCare", DosageForm = "Tablet" },
                    new() { Name = "Salbutamol", Description = "Asthma inhaler", Manufacturer = "RespiraMed", DosageForm = "Inhaler" },
                    new() { Name = "Vitamin D3", Description = "Vitamin D supplement", Manufacturer = "VitaHealth", DosageForm = "Tablet" },
                    new() { Name = "Cough Syrup", Description = "Relief from cough and cold", Manufacturer = "ColdCare", DosageForm = "Syrup" }
                };
                await context.Medicines.AddRangeAsync(medicines);
                await context.SaveChangesAsync();
            }

            // Seed Doctors
            if (!context.Doctors.Any())
            {
                var doctorUsers = new List<User>
                {
                    new() { Email = "dr.sharma@healthcare.com", PasswordHash = PasswordHasher.HashPassword("Doctor@123"), FirstName = "Dr. Rajesh", LastName = "Sharma", PhoneNumber = "9876543210", Role = UserRole.Doctor, IsActive = true },
                    new() { Email = "dr.patel@healthcare.com", PasswordHash = PasswordHasher.HashPassword("Doctor@123"), FirstName = "Dr. Priya", LastName = "Patel", PhoneNumber = "9876543211", Role = UserRole.Doctor, IsActive = true },
                    new() { Email = "dr.gupta@healthcare.com", PasswordHash = PasswordHasher.HashPassword("Doctor@123"), FirstName = "Dr. Anil", LastName = "Gupta", PhoneNumber = "9876543212", Role = UserRole.Doctor, IsActive = true },
                    new() { Email = "dr.khan@healthcare.com", PasswordHash = PasswordHasher.HashPassword("Doctor@123"), FirstName = "Dr. Fatima", LastName = "Khan", PhoneNumber = "9876543213", Role = UserRole.Doctor, IsActive = true },
                    new() { Email = "dr.iyer@healthcare.com", PasswordHash = PasswordHasher.HashPassword("Doctor@123"), FirstName = "Dr. Venkatesh", LastName = "Iyer", PhoneNumber = "9876543214", Role = UserRole.Doctor, IsActive = true },
                    new() { Email = "dr.desai@healthcare.com", PasswordHash = PasswordHasher.HashPassword("Doctor@123"), FirstName = "Dr. Meera", LastName = "Desai", PhoneNumber = "9876543215", Role = UserRole.Doctor, IsActive = true }
                };
                await context.Users.AddRangeAsync(doctorUsers);
                await context.SaveChangesAsync();

                var doctors = new List<Doctor>
                {
                    new() { UserId = doctorUsers[0].Id, LicenseNumber = "DOC001", Qualifications = "MBBS, MD (General Medicine)", ExperienceYears = 15, ConsultationFee = "500", AvailableDays = "Mon,Tue,Wed,Thu,Fri", AvailableFrom = TimeSpan.Parse("09:00"), AvailableTo = TimeSpan.Parse("17:00"), IsActive = true },
                    new() { UserId = doctorUsers[1].Id, LicenseNumber = "DOC002", Qualifications = "MBBS, MD (Cardiology)", ExperienceYears = 12, ConsultationFee = "800", AvailableDays = "Mon,Wed,Fri", AvailableFrom = TimeSpan.Parse("10:00"), AvailableTo = TimeSpan.Parse("16:00"), IsActive = true },
                    new() { UserId = doctorUsers[2].Id, LicenseNumber = "DOC003", Qualifications = "MBBS, MD (Dermatology)", ExperienceYears = 10, ConsultationFee = "600", AvailableDays = "Tue,Thu,Sat", AvailableFrom = TimeSpan.Parse("09:00"), AvailableTo = TimeSpan.Parse("15:00"), IsActive = true },
                    new() { UserId = doctorUsers[3].Id, LicenseNumber = "DOC004", Qualifications = "MBBS, MD (Pediatrics)", ExperienceYears = 8, ConsultationFee = "400", AvailableDays = "Mon,Tue,Wed,Thu,Fri", AvailableFrom = TimeSpan.Parse("09:00"), AvailableTo = TimeSpan.Parse("13:00"), IsActive = true },
                    new() { UserId = doctorUsers[4].Id, LicenseNumber = "DOC005", Qualifications = "MBBS, MS (Orthopedics)", ExperienceYears = 20, ConsultationFee = "700", AvailableDays = "Mon,Wed,Fri,Sat", AvailableFrom = TimeSpan.Parse("14:00"), AvailableTo = TimeSpan.Parse("18:00"), IsActive = true },
                    new() { UserId = doctorUsers[5].Id, LicenseNumber = "DOC006", Qualifications = "MBBS, MD (Neurology)", ExperienceYears = 18, ConsultationFee = "1000", AvailableDays = "Tue,Thu", AvailableFrom = TimeSpan.Parse("10:00"), AvailableTo = TimeSpan.Parse("16:00"), IsActive = true }
                };
                await context.Doctors.AddRangeAsync(doctors);
                await context.SaveChangesAsync();

                // Assign specializations to doctors
                var specs = await context.Specializations.ToListAsync();
                var doctorSpecs = new List<DoctorSpecialization>
                {
                    new() { DoctorId = doctors[0].Id, SpecializationId = specs[0].Id },
                    new() { DoctorId = doctors[1].Id, SpecializationId = specs[1].Id },
                    new() { DoctorId = doctors[2].Id, SpecializationId = specs[2].Id },
                    new() { DoctorId = doctors[3].Id, SpecializationId = specs[3].Id },
                    new() { DoctorId = doctors[4].Id, SpecializationId = specs[4].Id },
                    new() { DoctorId = doctors[5].Id, SpecializationId = specs[5].Id }
                };
                await context.DoctorSpecializations.AddRangeAsync(doctorSpecs);
                await context.SaveChangesAsync();
            }

            // Seed Patients
            if (context.Patients.Count() < 5)
            {
                var patientUsers = new List<User>
                {
                    new() { Email = "patient1@test.com", PasswordHash = PasswordHasher.HashPassword("Patient@123"), FirstName = "Rahul", LastName = "Kumar", PhoneNumber = "9123456789", Role = UserRole.Patient, IsActive = true },
                    new() { Email = "patient2@test.com", PasswordHash = PasswordHasher.HashPassword("Patient@123"), FirstName = "Sunita", LastName = "Verma", PhoneNumber = "9123456790", Role = UserRole.Patient, IsActive = true },
                    new() { Email = "patient3@test.com", PasswordHash = PasswordHasher.HashPassword("Patient@123"), FirstName = "Amit", LastName = "Singh", PhoneNumber = "9123456791", Role = UserRole.Patient, IsActive = true },
                    new() { Email = "patient4@test.com", PasswordHash = PasswordHasher.HashPassword("Patient@123"), FirstName = "Neha", LastName = "Gupta", PhoneNumber = "9123456792", Role = UserRole.Patient, IsActive = true },
                    new() { Email = "patient5@test.com", PasswordHash = PasswordHasher.HashPassword("Patient@123"), FirstName = "Suresh", LastName = "Patel", PhoneNumber = "9123456793", Role = UserRole.Patient, IsActive = true }
                };
                await context.Users.AddRangeAsync(patientUsers);
                await context.SaveChangesAsync();

                var patients = new List<Patient>
                {
                    new() { UserId = patientUsers[0].Id, DateOfBirth = new DateTime(1990, 5, 15), Gender = Gender.Male, Address = "123 Main St, Delhi", BloodGroup = "A+", MedicalHistory = "No major issues", Allergies = "None", EmergencyContactName = "Father", EmergencyContactPhone = "9123456788", IsActive = true },
                    new() { UserId = patientUsers[1].Id, DateOfBirth = new DateTime(1985, 8, 22), Gender = Gender.Female, Address = "456 Park Ave, Mumbai", BloodGroup = "B+", MedicalHistory = "Diabetes Type 2", Allergies = "Penicillin", EmergencyContactName = "Husband", EmergencyContactPhone = "9123456787", IsActive = true },
                    new() { UserId = patientUsers[2].Id, DateOfBirth = new DateTime(1995, 3, 10), Gender = Gender.Male, Address = "789 Lake Rd, Bangalore", BloodGroup = "O+", MedicalHistory = "Asthma", Allergies = "Dust", EmergencyContactName = "Mother", EmergencyContactPhone = "9123456786", IsActive = true },
                    new() { UserId = patientUsers[3].Id, DateOfBirth = new DateTime(1988, 11, 5), Gender = Gender.Female, Address = "321 Hill St, Chennai", BloodGroup = "AB+", MedicalHistory = "Hypertension", Allergies = "None", EmergencyContactName = "Sister", EmergencyContactPhone = "9123456785", IsActive = true },
                    new() { UserId = patientUsers[4].Id, DateOfBirth = new DateTime(1975, 7, 20), Gender = Gender.Male, Address = "654 River Rd, Kolkata", BloodGroup = "O-", MedicalHistory = "Heart Disease", Allergies = "Sulfa drugs", EmergencyContactName = "Son", EmergencyContactPhone = "9123456784", IsActive = true }
                };
                await context.Patients.AddRangeAsync(patients);
                await context.SaveChangesAsync();

                // Create some appointments
                var doctors = await context.Doctors.Include(d => d.User).ToListAsync();
                var appointments = new List<Appointment>
                {
                    new() { PatientId = patients[0].Id, DoctorId = doctors[0].Id, AppointmentDate = DateTime.Now.AddDays(1), AppointmentTime = TimeSpan.Parse("10:00"), DurationMinutes = 30, Status = AppointmentStatus.Pending, Reason = "Fever and headache", IsActive = true },
                    new() { PatientId = patients[1].Id, DoctorId = doctors[1].Id, AppointmentDate = DateTime.Now.AddDays(2), AppointmentTime = TimeSpan.Parse("11:00"), DurationMinutes = 45, Status = AppointmentStatus.Confirmed, Reason = "Chest pain consultation", IsActive = true },
                    new() { PatientId = patients[2].Id, DoctorId = doctors[2].Id, AppointmentDate = DateTime.Now.AddDays(1), AppointmentTime = TimeSpan.Parse("14:00"), DurationMinutes = 30, Status = AppointmentStatus.Pending, Reason = "Skin rash", IsActive = true },
                    new() { PatientId = patients[3].Id, DoctorId = doctors[3].Id, AppointmentDate = DateTime.Now.AddDays(3), AppointmentTime = TimeSpan.Parse("09:30"), DurationMinutes = 30, Status = AppointmentStatus.Pending, Reason = "Child vaccination", IsActive = true },
                    new() { PatientId = patients[4].Id, DoctorId = doctors[4].Id, AppointmentDate = DateTime.Now.AddDays(2), AppointmentTime = TimeSpan.Parse("15:00"), DurationMinutes = 60, Status = AppointmentStatus.Confirmed, Reason = "Knee pain", IsActive = true },
                    new() { PatientId = patients[0].Id, DoctorId = doctors[0].Id, AppointmentDate = DateTime.Now.AddDays(-2), AppointmentTime = TimeSpan.Parse("11:00"), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Regular checkup", IsActive = true },
                    new() { PatientId = patients[1].Id, DoctorId = doctors[2].Id, AppointmentDate = DateTime.Now.AddDays(-1), AppointmentTime = TimeSpan.Parse("10:00"), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Acne treatment", IsActive = true },
                    new() { PatientId = patients[2].Id, DoctorId = doctors[0].Id, AppointmentDate = DateTime.Now.AddDays(5), AppointmentTime = TimeSpan.Parse("16:00"), DurationMinutes = 30, Status = AppointmentStatus.Pending, Reason = "Cough and cold", IsActive = true }
                };
                await context.Appointments.AddRangeAsync(appointments);
                await context.SaveChangesAsync();

                // Create prescriptions for completed appointments
                var medicines = await context.Medicines.ToListAsync();
                var prescriptions = new List<Prescription>
                {
                    new() { AppointmentId = appointments[5].Id, PatientId = patients[0].Id, DoctorId = doctors[0].Id, Diagnosis = "Viral fever with headache", Advice = "Take rest and plenty of fluids", IsActive = true },
                    new() { AppointmentId = appointments[6].Id, PatientId = patients[1].Id, DoctorId = doctors[2].Id, Diagnosis = "Acne vulgaris", Advice = "Apply ointment twice daily", IsActive = true }
                };
                await context.Prescriptions.AddRangeAsync(prescriptions);
                await context.SaveChangesAsync();

                // Add prescription medicines
                var prescriptionMeds = new List<PrescriptionMedicine>
                {
                    new() { PrescriptionId = prescriptions[0].Id, MedicineId = medicines[0].Id, Dosage = "500mg", Frequency = "3 times daily", Duration = "3 days", Instructions = "After food", IsActive = true },
                    new() { PrescriptionId = prescriptions[0].Id, MedicineId = medicines[2].Id, Dosage = "400mg", Frequency = "2 times daily", Duration = "3 days", Instructions = "After food", IsActive = true },
                    new() { PrescriptionId = prescriptions[1].Id, MedicineId = medicines[2].Id, Dosage = "200mg", Frequency = "2 times daily", Duration = "7 days", Instructions = "Apply on affected area", IsActive = true }
                };
                await context.PrescriptionMedicines.AddRangeAsync(prescriptionMeds);
                await context.SaveChangesAsync();
            }
        }
    }
}
