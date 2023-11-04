using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TheCompleteBookOfMormon.Domain.Pages;

internal class PagePersistence : IEntityTypeConfiguration<Page>
{
    public void Configure(EntityTypeBuilder<Page> entity)
    {
        entity.HasOne<Editions.Edition>().WithMany().HasForeignKey(x => x.EditionId);
        entity.HasIndex(x => x.Number);
        entity.HasOne(x => x.Scan).WithOne();
    }
}
