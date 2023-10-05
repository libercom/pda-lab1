using MangaStore.Inventory.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MangaStore.Inventory.Context.Configurations
{
    public class MangaStockConfiguration : IEntityTypeConfiguration<MangaStock>
    {
        public void Configure(EntityTypeBuilder<MangaStock> builder)
        {
            builder.HasKey(x => new
            {
                x.MangaId,
                x.LocationId
            });
        }
    }
}
