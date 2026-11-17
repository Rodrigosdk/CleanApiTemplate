using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Template.Api.Errors;
using Template.Api.Logging;
using Template.Exception;
using Template.Exception.Exception;

namespace Template.Api.Filters;

public class ExceptionFilter(ProblemDetailsFactory problemDetailsFactory, ILogger<ExceptionFilter> logger) : IExceptionFilter
{
    public const int StatusClientClosedRequest = 499;

    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case TemplateException appException:
                SetErrorResult(context, (int)appException.GetStatusCode(), appException.GetErrorMessages());
                break;
            case BadHttpRequestException badHttpRequestException:
                SetErrorResult(context, badHttpRequestException.StatusCode, [ResourcesMessageException.INVALID_REQUEST]);
                break;
            case OperationCanceledException when context.HttpContext.RequestAborted.IsCancellationRequested:
                logger.RequestCancelled(context.HttpContext.Request.Method, context.HttpContext.Request.Path.Value);
                context.Result = new StatusCodeResult(StatusClientClosedRequest);
                break;
            default:
                // Detalhes de erros inesperados ficam só no log, nunca na resposta
                logger.UnhandledException(context.Exception, context.HttpContext.Request.Method, context.HttpContext.Request.Path.Value);
                SetErrorResult(context, StatusCodes.Status500InternalServerError, [ResourcesMessageException.UNKNOWN_ERROR]);
                break;
        }
    }

    private void SetErrorResult(ExceptionContext context, int statusCode, List<string> errors)
    {
        var problem = problemDetailsFactory.CreateProblemDetails(context.HttpContext, statusCode);
        problem.Extensions[ProblemDetailsSetup.ErrorsKey] = errors;

        context.Result = new ObjectResult(problem)
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" }
        };
    }
}
