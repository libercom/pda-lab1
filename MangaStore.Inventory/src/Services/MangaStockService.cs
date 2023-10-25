using MangaStore.Inventory.Context;
using MangaStore.Inventory.Dtos;
using MangaStore.Inventory.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaStore.Inventory.Services
{
    public class MangaStockService
    {
        private readonly InventoryContext _context;

        public MangaStockService(InventoryContext context)
        {
            _context = context;
        }

        public async Task<List<MangaStock>> GetByMangaIdAsync(Guid mangaId)
        {
            return await _context.MangaStocks
                .Include(x => x.Location)
                .Where(x => x.MangaId == mangaId)
                .ToListAsync();
        }

        public async Task AddMangaStockAsync(MangaStock mangaStock)
        {
            _context.Add(mangaStock);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateMangaStockAsync(MangaStock mangaStock)
        {
            var mangaStockToUpdate = await _context.MangaStocks
                .FirstOrDefaultAsync(x => x.MangaId == mangaStock.MangaId && x.LocationId == mangaStock.LocationId);

            if (mangaStockToUpdate == null)
            {
                return;
            }

            mangaStockToUpdate.Quantity = mangaStock.Quantity;
            mangaStockToUpdate.RestockDate = mangaStock.RestockDate;

            _context.Update(mangaStockToUpdate);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveMangaStockAsync(Guid mangaId)
        {
            var mangaStocks = await GetByMangaIdAsync(mangaId);

            _context.RemoveRange(mangaStocks);
            await _context.SaveChangesAsync();
        }

        public async Task SyncMangaStockAsync(Guid mangaId)
        {
            var locations = await _context.Locations.ToListAsync();

            foreach (var location in locations)
            {
                MangaStock mangaStock = new()
                {
                    MangaId = mangaId,
                    LocationId = location.Id,
                    Quantity = 0,
                    RestockDate = DateTime.UtcNow

                };

                _context.Add(mangaStock);
            }

            await _context.SaveChangesAsync();
        }
    }
}
