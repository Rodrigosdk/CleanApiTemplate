using Microsoft.Extensions.Logging;

namespace Template.Application.Logging;

// Catálogo dos eventos de log de negócio. O texto é um modelo fixo (não traduzir): é por ele e pelo
// EventId que os logs são agrupados e filtrados. Registre apenas IDs e valores, nunca dados pessoais.
// Reserve uma faixa de EventId para cada contexto (ex.: 1xxx usuários).
internal static partial class ApplicationLogs
{
#if (auth)
    // Usuários

    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Usuário {UserId} cadastrado")]
    public static partial void UserRegistered(this ILogger logger, Guid userId);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Usuário {UserId} autenticado")]
    public static partial void UserLoggedIn(this ILogger logger, Guid userId);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Warning, Message = "Tentativa de login com credenciais inválidas")]
    public static partial void UserLoginFailed(this ILogger logger);
#endif
}
