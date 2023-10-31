using Microsoft.EntityFrameworkCore;

namespace TheCompleteBookOfMormon.Domain.Editions;

public interface IEditionsRepository
{
    void Delete(Edition edition);
    ValueTask<Edition?> GetById(Guid id);
    void Save(Edition edition);
}

internal class EditionsRepository : RepositoryBase<Edition>, IEditionsRepository
{
    public EditionsRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    protected override DbSet<Edition> DbSet => DbContext.Editions;
}
