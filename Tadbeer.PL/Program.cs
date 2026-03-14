using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Scalar.AspNetCore;
using Tadbeer.DAL.Data;
using Tadbeer.DAL.Models;
using Tadbeer.PL.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

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

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
            //.WithOrigins("https://localhost:7139", "http://localhost:5129")
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
        // If you use cookies/auth across origins, you'll also need:
        // .AllowCredentials();
    });
});

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
// CORS (if needed) should generally run before auth
app.UseCors(userPolicy);
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();

app.Run();