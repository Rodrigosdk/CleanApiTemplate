using System.Net;

namespace Template.Exception.Exception;

public abstract class TemplateException(string message) : System.Exception(message)
{
    public abstract HttpStatusCode GetStatusCode();
    public abstract List<string> GetErrorMessages();
}
