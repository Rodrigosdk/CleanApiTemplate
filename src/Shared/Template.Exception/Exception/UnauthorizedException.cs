using System.Net;

namespace Template.Exception.Exception;

public class UnauthorizedException(string message) : TemplateException(message)
{
    public override HttpStatusCode GetStatusCode()
    {
        return HttpStatusCode.Unauthorized;
    }

    public override List<string> GetErrorMessages()
    {
        return [Message];
    }
}
