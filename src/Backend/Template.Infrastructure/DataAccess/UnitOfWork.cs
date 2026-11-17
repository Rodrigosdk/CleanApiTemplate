using Microsoft.EntityFrameworkCore;
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
    }
}
