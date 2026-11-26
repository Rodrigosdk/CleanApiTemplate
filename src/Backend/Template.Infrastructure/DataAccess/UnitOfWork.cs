using Microsoft.EntityFrameworkCore;
#if (auth)
using Template.Infrastructure.DataAccess.Mappings;
#endif
using Template.Domain.Repositories;
using Template.Exception;
using Template.Exception.Exception;

namespace Template.Infrastructure.DataAccess;

internal class UnitOfWork(TemplateDbContext dbContext) : IUnitOfWork
{
#if (sqlserver && auth)
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;

#elif (mysql && auth)
    private const int DuplicateEntry = 1062;

#endif
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
#if (postgres)
        exception.InnerException is Npgsql.PostgresException { SqlState: Npgsql.PostgresErrorCodes.UniqueViolation } postgresException &&
        postgresException.ConstraintName == indexName;
#elif (sqlserver)
        exception.InnerException is Microsoft.Data.SqlClient.SqlException { Number: UniqueIndexViolation or UniqueConstraintViolation } sqlException &&
        sqlException.Message.Contains(indexName, StringComparison.Ordinal);
#else
        exception.InnerException is MySql.Data.MySqlClient.MySqlException { Number: DuplicateEntry } mySqlException &&
        mySqlException.Message.Contains(indexName, StringComparison.Ordinal);
#endif
#endif
}
