using Microsoft.EntityFrameworkCore;

namespace TheCompleteBookOfMormon.Domain.Editions;

public interface IEditionsRepository
{
    void Delete(Edition edition);
    ValueTask<Edition?> GetByCodeAsync(string? code);
    ValueTask<Edition?> GetByIdAsync(Guid id);
    void Save(Edition edition);
}

internal class EditionsRepository : RepositoryBase<Edition>, IEditionsRepository
{
    public EditionsRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    protected override DbSet<Edition> DbSet => DbContext.Editions;

    public async ValueTask<Edition?> GetByCodeAsync(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code required", nameof(code));

        return await DbSet.FirstOrDefaultAsync(x => x.Code == code);
    }
}
