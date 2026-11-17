using System.Net;

namespace Template.Exception.Exception;

public class NotFoundException(string message) : TemplateException(message)
{
    public override HttpStatusCode GetStatusCode()
    {
        return HttpStatusCode.NotFound;
    }

    public override List<string> GetErrorMessages()
    {
        return [Message];
    }
}
