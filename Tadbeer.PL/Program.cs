using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using Scalar.AspNetCore;
using Tadbeer.DAL.Data;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.Utilities;
using Tadbeer.PL.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<Tadbeer.PL.Filters.GlobalExceptionFilter>();
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// 1. Add DB Context first
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
    options.User.RequireUniqueEmail = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.SignIn.RequireConfirmedEmail = true;
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddMemoryCache(); // Required for CheckUserStatusMiddleware

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register custom mappings (Mapster)
Tadbeer.BLL.Profiles.MapsterConfig.RegisterMappings();

// Register all Repositories, UnitOfWork, and Services from the BLL/DAL
builder.Services.AddApplicationServices();

// Add CORS only if you need cross-origin browser requests
const string userPolicy = "UserPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(userPolicy, policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    return false;
                }

                // Allow local development from any localhost/loopback port.
                if (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                    uri.Host.Equals("127.0.0.1") ||
                    uri.Host.Equals("::1"))
                {
                    return true;
                }

                // Allow deployed frontend/backend host.
                return uri.Host.Equals("tadbeer0.onrender.com", StringComparison.OrdinalIgnoreCase);
            })
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.MapOpenApi();
    app.MapScalarApiReference();
//}

using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var seedData = scope.ServiceProvider.GetRequiredService<ISeedData>();

        // Run all seeds only once on first bootstrap in Development.
        var shouldRunInitialSeeding = app.Environment.IsDevelopment()
                                      && !await context.Specialties.AnyAsync()
                                      && !await context.Roles.AnyAsync()
                                      && !await context.Users.AnyAsync();

        if (shouldRunInitialSeeding)
        {
            await seedData.SpecialtiesDataSeedingAsync();
            await seedData.IdentityDataSeedingAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during DB migration or seeding.");
        throw;
    }
}

app.UseHttpsRedirection();

// CORS should run before auth so OPTIONS preflight succeeds.
app.UseCors(userPolicy);

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<Tadbeer.PL.Middlewares.CheckUserStatusMiddleware>();

app.UseStaticFiles();

app.MapControllers();
app.MapGet("/", () => "Hello World!");
app.Run();