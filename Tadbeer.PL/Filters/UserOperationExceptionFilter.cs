using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Tadbeer.BLL.Exceptions;

namespace Tadbeer.PL.Filters;

public class UserOperationExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is UserOperationException ex)
        {
            context.Result = new BadRequestObjectResult(new { message = ex.Message });
            context.ExceptionHandled = true;
        }
    }
}
