using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tadbeer.DAL.Data;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.Repositories.Classes;
using Tadbeer.DAL.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add CORS only if you need cross-origin browser requests
const string userPolicy = "UserPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(userPolicy, policy =>
    {
        policy
            .WithOrigins("https://localhost:5173", "http://localhost:5173")
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
}

app.UseHttpsRedirection();

// CORS (if needed) should generally run before auth
app.UseCors(userPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.Run();