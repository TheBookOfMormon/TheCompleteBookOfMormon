using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TheCompleteBookOfMormon.Domain.Editions;

internal class EditionPersistence : IEntityTypeConfiguration<Edition>
{
    public void Configure(EntityTypeBuilder<Edition> entity)
    {
        entity.HasIndex(x => x.Code).IsUnique();
        entity.HasIndex(x => x.Name).IsUnique();
    }
}
