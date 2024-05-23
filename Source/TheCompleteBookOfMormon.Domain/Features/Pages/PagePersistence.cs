using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheCompleteBookOfMormon.Domain.Features.Editions;

namespace TheCompleteBookOfMormon.Domain.Features.Pages;

internal class PagePersistence : IEntityTypeConfiguration<Page>
{
    public void Configure(EntityTypeBuilder<Page> entity)
    {
        entity.HasOne<Edition>().WithMany().HasForeignKey(x => x.EditionId);
        entity.HasIndex(x => x.Number);
    }
}
