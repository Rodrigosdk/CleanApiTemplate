namespace Template.Communication.Responses;

public class ResponseRegisteredUserJson
{
    public string Nome { get; set; } = string.Empty;

    public ResponseTokenJson Tokens { get; set; } = new();
}

