using System.Net;

namespace Template.Exception.Exception;

// Uma regra de negócio do domínio foi violada (dado inválido ou operação não permitida no estado atual)
public class DomainRuleException(string message) : TemplateException(message)
{
    public override HttpStatusCode GetStatusCode()
    {
        return HttpStatusCode.BadRequest;
    }

    public override List<string> GetErrorMessages()
    {
        return [Message];
    }
}
