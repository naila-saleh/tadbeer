using System.Net;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Tadbeer.BLL.Exceptions;

namespace Tadbeer.PL.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = context.Response;
        object errorResponse = null!;

        switch (exception)
        {
            case DuplicateSpecialtyException duplicateEx:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                errorResponse = new { message = duplicateEx.Message };
                break;

            case DbUpdateException dbEx when IsUniqueConstraintViolation(dbEx):
                response.StatusCode = (int)HttpStatusCode.Conflict;
                errorResponse = new { message = ResolveUniqueViolationMessage(dbEx) };
                break;

            case UserOperationException userEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = new { message = userEx.Message };
                break;

            default:
                _logger.LogError(exception, "An unhandled exception occurred.");
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse = new { message = "An unexpected error occurred. Please try again later." };
                break;
        }

        var result = JsonSerializer.Serialize(errorResponse);
        await response.WriteAsync(result);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlEx &&
               (sqlEx.Number == 2601 || sqlEx.Number == 2627);
    }

    private static string ResolveUniqueViolationMessage(DbUpdateException exception)
    {
        var details = exception.InnerException?.Message ?? exception.Message;

        if (details.Contains("PhoneNumbers", StringComparison.OrdinalIgnoreCase)
            || details.Contains("IX_PhoneNumbers_Number", StringComparison.OrdinalIgnoreCase))
        {
            return "Phone number already exists.";
        }

        if (details.Contains("Specialties", StringComparison.OrdinalIgnoreCase)
            || details.Contains("IX_Specialties_Name", StringComparison.OrdinalIgnoreCase))
        {
            return "Specialty already exists.";
        }

        return "A duplicate value already exists.";
    }
}
