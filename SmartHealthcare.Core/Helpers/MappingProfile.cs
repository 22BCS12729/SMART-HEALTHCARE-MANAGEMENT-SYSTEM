using AutoMapper;
using SmartHealthcare.Core.DTOs;
using SmartHealthcare.Core.Entities;

namespace SmartHealthcare.Core.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, UserDto>();
            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.LastLoginAt, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshTokenExpiryTime, opt => opt.Ignore())
                .ForMember(dest => dest.Patient, opt => opt.Ignore())
                .ForMember(dest => dest.Doctor, opt => opt.Ignore());

            // Patient mappings
            CreateMap<Patient, PatientDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber));

            CreateMap<CreatePatientDto, Patient>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.Appointments, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());

            CreateMap<UpdatePatientDto, Patient>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Doctor mappings
            CreateMap<Doctor, DoctorDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.Specializations, opt => opt.MapFrom(src => src.DoctorSpecializations.Select(ds => ds.Specialization)));

            CreateMap<Doctor, DoctorListDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.SpecializationNames, opt => opt.MapFrom(src => src.DoctorSpecializations.Select(ds => ds.Specialization.Name)));

            CreateMap<CreateDoctorDto, Doctor>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.Appointments, opt => opt.Ignore())
                .ForMember(dest => dest.Prescriptions, opt => opt.Ignore())
                .ForMember(dest => dest.DoctorSpecializations, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());

            CreateMap<UpdateDoctorDto, Doctor>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Specialization mappings
            CreateMap<Specialization, SpecializationDto>()
                .ForMember(dest => dest.DoctorCount, opt => opt.MapFrom(src => src.DoctorSpecializations.Count));

            CreateMap<CreateSpecializationDto, Specialization>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.DoctorSpecializations, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));

            CreateMap<UpdateSpecializationDto, Specialization>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Specialization, SpecializationWithDoctorsDto>();

            // Appointment mappings
            CreateMap<Appointment, AppointmentDto>()
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient.User.FullName))
                .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor.User.FullName))
                .ForMember(dest => dest.PrescriptionId, opt => opt.MapFrom(src => src.Prescription != null ? (int?)src.Prescription.Id : null));

            CreateMap<CreateAppointmentDto, Appointment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Patient, opt => opt.Ignore())
                .ForMember(dest => dest.PatientId, opt => opt.Ignore())
                .ForMember(dest => dest.Doctor, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => Enums.AppointmentStatus.Pending))
                .ForMember(dest => dest.Prescription, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));

            CreateMap<UpdateAppointmentDto, Appointment>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Prescription mappings
            CreateMap<Prescription, PrescriptionDto>()
                .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor.User.FullName))
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient.User.FullName))
                .ForMember(dest => dest.Medicines, opt => opt.MapFrom(src => src.PrescriptionMedicines));

            CreateMap<Prescription, PrescriptionListDto>()
                .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor.User.FullName))
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient.User.FullName))
                .ForMember(dest => dest.MedicineCount, opt => opt.MapFrom(src => src.PrescriptionMedicines.Count));

            CreateMap<CreatePrescriptionDto, Prescription>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Appointment, opt => opt.Ignore())
                .ForMember(dest => dest.Doctor, opt => opt.Ignore())
                .ForMember(dest => dest.Patient, opt => opt.Ignore())
                .ForMember(dest => dest.PrescriptionMedicines, opt => opt.Ignore())
                .ForMember(dest => dest.PrescriptionDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));

            CreateMap<UpdatePrescriptionDto, Prescription>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // PrescriptionMedicine mappings
            CreateMap<PrescriptionMedicine, PrescriptionMedicineDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Medicine.Id))
                .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Medicine.Name));

            CreateMap<CreatePrescriptionMedicineDto, PrescriptionMedicine>()
                .ForMember(dest => dest.Prescription, opt => opt.Ignore())
                .ForMember(dest => dest.Medicine, opt => opt.Ignore());

            // Medicine mappings
            CreateMap<Medicine, MedicineDto>();
            CreateMap<Medicine, MedicineListDto>();

            CreateMap<CreateMedicineDto, Medicine>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PrescriptionMedicines, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));

            CreateMap<UpdateMedicineDto, Medicine>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
