namespace SmartHealthcare.Core.Enums
{
    public enum UserRole
    {
        Admin = 1,
        Doctor = 2,
        Patient = 3
    }

    public enum AppointmentStatus
    {
        Pending = 1,
        Confirmed = 2,
        Completed = 3,
        Cancelled = 4,
        NoShow = 5
    }

    public enum Gender
    {
        Male = 1,
        Female = 2,
        Other = 3
    }
}
