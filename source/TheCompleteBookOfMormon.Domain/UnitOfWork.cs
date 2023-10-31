namespace TheCompleteBookOfMormon.Domain;

public interface IUnitOfWork
{
    ValueTask CommitAsync(CancellationToken cancellationToken = default);
}

internal class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext DbContext;

    public UnitOfWork(ApplicationDbContext dbContext)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        dbContext.EnableChangeTracking();
    }

    public async ValueTask CommitAsync(CancellationToken cancellationToken = default) =>
        await DbContext.SaveChangesAsync(cancellationToken);
}
