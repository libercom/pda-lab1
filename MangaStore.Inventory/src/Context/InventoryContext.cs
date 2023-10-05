using MangaStore.Inventory.Context.Configurations;
using MangaStore.Inventory.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaStore.Inventory.Context
{
    public class InventoryContext : DbContext
    {
        public DbSet<MangaStock> MangaStocks { get; set; } = null!;
        public DbSet<Location> Locations { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;

        public InventoryContext(DbContextOptions<InventoryContext> dbContextOptions) : base(dbContextOptions)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            new MangaStockConfiguration().Configure(modelBuilder.Entity<MangaStock>());
            new LocationConfiguration().Configure(modelBuilder.Entity<Location>());
            new OrderConfiguration().Configure(modelBuilder.Entity<Order>());

            base.OnModelCreating(modelBuilder);
        }
    }
}
