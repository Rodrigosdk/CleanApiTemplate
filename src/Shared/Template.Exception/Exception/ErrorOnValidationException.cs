using System.Net;

namespace Template.Exception.Exception;

public class ErrorOnValidationException(List<string> errors) : TemplateException(string.Join("; ", errors))
{
    public ErrorOnValidationException(string error) : this([error])
    {
    }

    public override HttpStatusCode GetStatusCode()
    {
        return HttpStatusCode.BadRequest;
    }

    public override List<string> GetErrorMessages()
    {
        return errors;
    }
}
