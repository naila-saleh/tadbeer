using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Scalar.AspNetCore;
using Tadbeer.DAL.Data;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.Utilities;
using Tadbeer.PL.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<Tadbeer.PL.Filters.GlobalExceptionFilter>();
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
                if(!Uri.TryCreate(origin, UriKind.Absolute, out var uri))return false;
                if(uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || uri.Host.Equals("127.0.0.1") || uri.Host.Equals("::1")) return true;
                return uri.Host.Equals("https://tadbeer0.onrender.com", StringComparison.OrdinalIgnoreCase) || uri.Host.Equals("http://tadbeer0.onrender.com", StringComparison.OrdinalIgnoreCase);
            })
            .AllowAnyHeader()
            .AllowAnyMethod();
        // If you use cookies/auth across origins, you'll also need:
        // .AllowCredentials();
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
        await seedData.DataSeedingAsync();
        await seedData.IdentityDataSeedingAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during DB migration or seeding.");
    }
}

app.UseHttpsRedirection();

app.UseAuthentication();
// CORS (if needed) should generally run before auth
app.UseCors(userPolicy);
app.UseAuthorization();
app.UseMiddleware<Tadbeer.PL.Middlewares.CheckUserStatusMiddleware>();

app.UseStaticFiles();

app.MapControllers();
app.MapGet("/", () => "Hello World!");
app.Run();