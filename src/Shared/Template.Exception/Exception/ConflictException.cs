using System.Net;

namespace Template.Exception.Exception;

// O registro foi alterado por outra operação entre a leitura e a gravação
public class ConflictException(string message) : TemplateException(message)
{
    public override HttpStatusCode GetStatusCode()
    {
        return HttpStatusCode.Conflict;
    }

    public override List<string> GetErrorMessages()
    {
        return [Message];
    }
}
