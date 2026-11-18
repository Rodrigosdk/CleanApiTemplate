using Microsoft.EntityFrameworkCore;
using Npgsql;
using Template.Infrastructure.DataAccess.Mappings;
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
        catch (DbUpdateException exception) when (IsUniqueViolation(exception, UserMap.ActiveEmailIndex))
        {
            // Duas operações simultâneas passaram pela verificação de e-mail; o índice barrou a segunda
            throw new ErrorOnValidationException(ResourcesMessageException.USER_ACTIVE_EMAIL_EXISTS);
        }
    }

    private static bool IsUniqueViolation(DbUpdateException exception, string indexName) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } postgresException &&
        postgresException.ConstraintName == indexName;
}
