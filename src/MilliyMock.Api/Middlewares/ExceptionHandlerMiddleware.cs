using MilliyMock.Domain.Exceptions;
using MilliyMock.Models;

namespace MilliyMock.Middlewares;

public class ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch(MilliyMockException exception)
        {
            context.Response.StatusCode = exception.Code;
            await context.Response.WriteAsJsonAsync(new Response
            {
                Code = exception.Code,
                Message = exception.Message
            });
        }
        catch (Exception exception)
        {
            logger.LogError($"{exception}\n\n");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new Response
            {
                Code = 500,
                Message = exception.Message 
            });
        }
    }
}

