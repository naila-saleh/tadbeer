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
            context.Result = new ConflictObjectResult(new
            {
                message = "Specialty already exists."
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
}
