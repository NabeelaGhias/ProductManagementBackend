using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProductManagementSystem.Domain.Data;
using ProductManagementSystem.Domain.Entities;
using ProductManagementSystem.Domain.Repository;
using ProductManagementSystem.Domain.Settings;
using ProductManagementSystem.Interfaces;
using ProductManagementSystem.Services;
using ProductManagementSystem.Cache;
using ProductManagementSystem.Middleware;
using System.Text;
using Serilog;
using Serilog.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
IConfiguration configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Staging"}.json", true)
        .Build();

var serilogLogger = new LoggerConfiguration()
.ReadFrom.Configuration(configuration)
.Enrich.FromLogContext()
.CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(serilogLogger);

Log.Logger = serilogLogger;

// Add services to the container.
builder.Services.AddSingleton(Log.Logger);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200") // Angular development server
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Database Context
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
            sqlOptions.CommandTimeout(30);
            sqlOptions.MigrationsAssembly(typeof(Program).Assembly.GetName().Name);
            // Use legacy timestamp behavior to allow non-UTC dates
            sqlOptions.SetPostgresVersion(new Version(9, 6));
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        });
});

// Identity Configuration
builder.Services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    
    // SignIn settings
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders()
.AddRoles<IdentityRole<Guid>>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!))
    };
});

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Repository Pattern
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOtpService, OtpService>();

// Configuration Settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<OtpSettings>(builder.Configuration.GetSection("OtpSettings"));

// Caching
builder.Services.AddMemoryCache();

var app = builder.Build();

// Ensure Database Created and Apply Migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<DataContext>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        // Create database if it doesn't exist
        await context.Database.EnsureCreatedAsync();
        
        // Apply any pending migrations
        if ((await context.Database.GetPendingMigrationsAsync()).Any())
        {
            await context.Database.MigrateAsync();
        }

        // Seed Data
        await DbSeeder.SeedData(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger1 = services.GetRequiredService<ILogger<Program>>();
        logger1.LogError(ex, "An error occurred while migrating or initializing the database.");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Custom Middleware (order is important)
app.UseMiddleware<ExceptionMiddleware>();  // First to catch all exceptions
app.UseMiddleware<ProductManagementSystem.Middlewares.LoggingMiddleware>();    // Second to log all requests

app.UseHttpsRedirection();

// Enable static files serving
app.UseStaticFiles();

// Enable CORS
app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    app.Run();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}