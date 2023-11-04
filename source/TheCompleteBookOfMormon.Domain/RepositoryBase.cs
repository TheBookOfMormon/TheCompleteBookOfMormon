using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace TheCompleteBookOfMormon.Domain;

public abstract class RepositoryBase<T> where T : Entity, new()
{
    protected readonly ApplicationDbContext DbContext;
    protected abstract DbSet<T> DbSet { get; }

    protected RepositoryBase(ApplicationDbContext dbContext)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public void Delete(T entity)
    {
        DbSet.Attach(entity);
        DbSet.Remove(entity);
    }

    public void Delete(Guid id)
    {
        Delete(new T() {  Id = id });
    }

    public async ValueTask<T?> GetByIdAsync(Guid id) =>
        await DbSet.FirstOrDefaultAsync(x => x.Id == id);

    public void Save(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        EntityEntry<T> entityEntry = DbContext.Entry(entity);
        if (entityEntry is null || entityEntry.State == EntityState.Detached)
            DbSet.Add(entity);
        else if (entityEntry.State == EntityState.Unchanged)
            entityEntry.State = EntityState.Modified;
    }

    public void Save(IEnumerable<T> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        foreach (T entity in entities)
            Save(entity!);
    }

}
