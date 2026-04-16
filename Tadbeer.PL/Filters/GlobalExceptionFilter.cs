using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Tadbeer.BLL.Exceptions;

namespace Tadbeer.PL.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is DuplicateSpecialtyException duplicateEx)
        {
            context.Result = new ConflictObjectResult(new { message = duplicateEx.Message });
            context.ExceptionHandled = true;
            return;
        }

        if (context.Exception is DbUpdateException dbEx &&
            IsUniqueConstraintViolation(dbEx))
        {
            var message = ResolveUniqueViolationMessage(dbEx);
            context.Result = new ConflictObjectResult(new
            {
                message
            });
            context.ExceptionHandled = true;
            return;
        }

        if (context.Exception is UserOperationException userEx)
        {
            context.Result = new BadRequestObjectResult(new { message = userEx.Message });
            context.ExceptionHandled = true;
        }
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
