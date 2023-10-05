using MangaStore.Inventory.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MangaStore.Inventory.Context.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder
                .HasOne(x => x.MangaStock)
                .WithMany()
                .HasForeignKey(x => new { x.MangaId, x.LocationId });
        }
    }
}
