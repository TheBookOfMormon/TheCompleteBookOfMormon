using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace TheCompleteBookOfMormon.Domain.Features.Pages;

public interface IPageRepository
{
    void Delete(Page page);
    ValueTask<Page?> GetAsync(Guid editionId, int pageNumber);
    ValueTask<ImmutableArray<Page>> GetAllAsync(Guid editionId);
    void Save(Page scan);
}

internal class PageRepository : RepositoryBase<Page>, IPageRepository
{
    protected override DbSet<Page> DbSet => DbContext.Pages;

    public PageRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async ValueTask<Page?> GetAsync(Guid editionId, int pageNumber)
    {
        return await DbSet
            .Where(x => x.EditionId == editionId)
            .Where(x => x.Number == pageNumber)
            .FirstOrDefaultAsync();
    }

    public async ValueTask<ImmutableArray<Page>> GetAllAsync(Guid editionId)
    {
        Page[] pages = await DbSet
            .Where(x => x.EditionId == editionId)
            .ToArrayAsync();
        return pages.ToImmutableArray();
    }
}
