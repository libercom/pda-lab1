using MangaStore.Inventory.Context;
using MangaStore.Inventory.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaStore.Inventory.Services
{
    public class LocationService
    {
        private readonly InventoryContext _context;
        
        public LocationService(InventoryContext context)
        {
            _context = context;
        }

        public async Task<List<Location>> GetAllLocationsAsync()
        {
            return await _context.Locations.ToListAsync();
        }

        public async Task AddLocationAsync(Location location)
        {
            _context.Add(location);
            await _context.SaveChangesAsync();
        }
    }
}
