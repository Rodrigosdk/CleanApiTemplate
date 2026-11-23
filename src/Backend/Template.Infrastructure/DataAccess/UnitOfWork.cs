using Microsoft.EntityFrameworkCore;
#if (auth)
using Npgsql;
using Template.Infrastructure.DataAccess.Mappings;
#endif
using Template.Domain.Repositories;
using Template.Exception;
using Template.Exception.Exception;

namespace Template.Infrastructure.DataAccess;

internal class UnitOfWork(TemplateDbContext dbContext) : IUnitOfWork
{
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Outra operação alterou o registro depois que ele foi lido (versão diferente)
            throw new ConflictException(ResourcesMessageException.CONCURRENCY_CONFLICT);
        }
#if (auth)
        catch (DbUpdateException exception) when (IsUniqueViolation(exception, UserMap.ActiveEmailIndex))
        {
            // Duas operações simultâneas passaram pela verificação de e-mail; o índice barrou a segunda
            throw new ErrorOnValidationException(ResourcesMessageException.USER_ACTIVE_EMAIL_EXISTS);
        }
#endif
    }
#if (auth)

    private static bool IsUniqueViolation(DbUpdateException exception, string indexName) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } postgresException &&
        postgresException.ConstraintName == indexName;
#endif
}
