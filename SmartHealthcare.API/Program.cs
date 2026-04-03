using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartHealthcare.API.Middleware;
using SmartHealthcare.API.Services;
using SmartHealthcare.Core.Data;
using SmartHealthcare.Core.Helpers;
using SmartHealthcare.Core.Interfaces;
using SmartHealthcare.Core.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT authentication
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Smart Healthcare API", Version = "v1" });

    // Add JWT Security Definition
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token in the format: Bearer {your_token}"
    });

    // Add Security Requirement
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("SmartHealthcare.API")));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

// Configure Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("DoctorOnly", policy => policy.RequireRole("Doctor"));
    options.AddPolicy("PatientOnly", policy => policy.RequireRole("Patient"));
    options.AddPolicy("AdminOrDoctor", policy => policy.RequireRole("Admin", "Doctor"));
});

// Configure Caching - Use Redis if enabled, otherwise In-Memory
var cacheSettings = builder.Configuration.GetSection("CacheSettings");
var useRedis = cacheSettings.GetValue<bool>("UseRedis");

if (useRedis)
{
    var redisConnection = builder.Configuration.GetConnectionString("Redis");
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "SmartHealthcare_";
    });
    Console.WriteLine($"Redis caching enabled: {redisConnection}");
}
else
{
    builder.Services.AddDistributedMemoryCache();
    Console.WriteLine("In-memory caching enabled");
}

// Configure response caching
builder.Services.AddResponseCaching();

// Configure Memory Cache (for in-memory caching scenarios)
builder.Services.AddMemoryCache();

// Configure AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>(), typeof(MappingProfile).Assembly);

// Register JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Register Repository and Unit of Work
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Services
builder.Services.AddScoped<IAuthService, JwtService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<IMedicineService, MedicineService>();
builder.Services.AddScoped<ISpecializationService, SpecializationService>();

// Register Cache Service
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    options.AddPolicy("AllowMvcClient", policy =>
    {
        policy.WithOrigins(
                builder.Configuration["MvcClient:Url"] ?? "https://localhost:7001",
                "https://localhost:5001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

// Configure Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Healthcare API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Smart Healthcare API Documentation";
    });
}

// Use global exception handler
app.UseGlobalExceptionHandler();

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowMvcClient");

// Use Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map Health Checks
app.MapHealthChecks("/health");

// Map Controllers
app.MapControllers();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        dbContext.Database.Migrate();
        
        // Seed admin user password if needed
        var adminUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "admin@smarthealth.com");
        if (adminUser != null && string.IsNullOrEmpty(adminUser.PasswordHash))
        {
            adminUser.PasswordHash = SmartHealthcare.API.Helpers.PasswordHasher.HashPassword("Admin@123");
            await dbContext.SaveChangesAsync();
        }

        // Seed dummy data
        await DataSeeder.SeedAsync(dbContext);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

app.Run();
