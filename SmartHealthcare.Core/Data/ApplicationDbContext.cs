using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SmartHealthcare.Core.Entities;

namespace SmartHealthcare.Core.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<DoctorSpecialization> DoctorSpecializations { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<PrescriptionMedicine> PrescriptionMedicines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Email).HasMaxLength(255);
                entity.Property(u => u.FirstName).HasMaxLength(100);
                entity.Property(u => u.LastName).HasMaxLength(100);
                entity.Property(u => u.PhoneNumber).HasMaxLength(20);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.Role).HasConversion<string>();
            });

            // Configure One-to-One: User <-> Patient
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasOne(p => p.User)
                    .WithOne(u => u.Patient)
                    .HasForeignKey<Patient>(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(p => p.UserId).IsUnique();
                entity.Property(p => p.EmergencyContactPhone).HasMaxLength(20);
                entity.Property(p => p.Gender).HasConversion<string>();
                entity.Property(p => p.BloodGroup).HasMaxLength(10);
            });

            // Configure One-to-One: User <-> Doctor
            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithOne(u => u.Doctor)
                    .HasForeignKey<Doctor>(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(d => d.UserId).IsUnique();
                entity.HasIndex(d => d.LicenseNumber).IsUnique();
                entity.Property(d => d.LicenseNumber).HasMaxLength(100);
            });

            // Configure Specialization entity
            modelBuilder.Entity<Specialization>(entity =>
            {
                entity.HasIndex(s => s.Name).IsUnique();
                entity.Property(s => s.Name).HasMaxLength(100);
            });

            // Configure Many-to-Many: Doctor <-> Specialization
            modelBuilder.Entity<DoctorSpecialization>(entity =>
            {
                entity.HasKey(ds => ds.Id);
                entity.HasIndex(ds => new { ds.DoctorId, ds.SpecializationId }).IsUnique();

                entity.HasOne(ds => ds.Doctor)
                    .WithMany(d => d.DoctorSpecializations)
                    .HasForeignKey(ds => ds.DoctorId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ds => ds.Specialization)
                    .WithMany(s => s.DoctorSpecializations)
                    .HasForeignKey(ds => ds.SpecializationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Appointment entity
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasOne(a => a.Patient)
                    .WithMany(p => p.Appointments)
                    .HasForeignKey(a => a.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Doctor)
                    .WithMany(d => d.Appointments)
                    .HasForeignKey(a => a.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(a => a.Status).HasConversion<string>();
                entity.HasIndex(a => new { a.DoctorId, a.AppointmentDate, a.AppointmentTime });
                entity.HasIndex(a => a.PatientId);
                entity.HasIndex(a => a.AppointmentDate);
            });

            // Configure Prescription entity
            modelBuilder.Entity<Prescription>(entity =>
            {
                entity.HasOne(p => p.Appointment)
                    .WithOne(a => a.Prescription)
                    .HasForeignKey<Prescription>(p => p.AppointmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Doctor)
                    .WithMany(d => d.Prescriptions)
                    .HasForeignKey(p => p.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Patient)
                    .WithMany()
                    .HasForeignKey(p => p.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(p => p.AppointmentId).IsUnique();
            });

            // Configure Medicine entity
            modelBuilder.Entity<Medicine>(entity =>
            {
                entity.HasIndex(m => m.Name);
                entity.Property(m => m.Name).HasMaxLength(200);
                entity.Property(m => m.GenericName).HasMaxLength(200);
                entity.Property(m => m.Manufacturer).HasMaxLength(200);
                entity.Property(m => m.DosageForm).HasMaxLength(100);
            });

            // Configure Many-to-Many: Prescription <-> Medicine
            modelBuilder.Entity<PrescriptionMedicine>(entity =>
            {
                entity.HasKey(pm => pm.Id);
                entity.HasIndex(pm => new { pm.PrescriptionId, pm.MedicineId }).IsUnique();

                entity.HasOne(pm => pm.Prescription)
                    .WithMany(p => p.PrescriptionMedicines)
                    .HasForeignKey(pm => pm.PrescriptionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pm => pm.Medicine)
                    .WithMany(m => m.PrescriptionMedicines)
                    .HasForeignKey(pm => pm.MedicineId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(pm => pm.Dosage).HasMaxLength(100);
                entity.Property(pm => pm.Frequency).HasMaxLength(100);
                entity.Property(pm => pm.Duration).HasMaxLength(100);
            });

            // Seed initial data
            SeedInitialData(modelBuilder);
        }

        private void SeedInitialData(ModelBuilder modelBuilder)
        {
            // Seed Specializations
            modelBuilder.Entity<Specialization>().HasData(
                new Specialization { Id = 1, Name = "Cardiology", Description = "Heart and cardiovascular system" },
                new Specialization { Id = 2, Name = "Dermatology", Description = "Skin, hair, and nail conditions" },
                new Specialization { Id = 3, Name = "Neurology", Description = "Brain and nervous system" },
                new Specialization { Id = 4, Name = "Pediatrics", Description = "Children's health" },
                new Specialization { Id = 5, Name = "Orthopedics", Description = "Musculoskeletal system" },
                new Specialization { Id = 6, Name = "General Medicine", Description = "General healthcare" }
            );

            // Seed Admin User (password will be hashed during creation)
            var adminUser = new User
            {
                Id = 1,
                FirstName = "System",
                LastName = "Administrator",
                Email = "admin@smarthealth.com",
                PasswordHash = "", // Will be set during service initialization
                Role = Enums.UserRole.Admin,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1)
            };

            // Note: Password should be set properly in the service layer
            modelBuilder.Entity<User>().HasData(adminUser);
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}
