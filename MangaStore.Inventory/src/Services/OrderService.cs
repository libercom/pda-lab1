using MangaStore.Inventory.Context;
using MangaStore.Inventory.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaStore.Inventory.Services
{
    public class OrderService
    {
        private readonly InventoryContext _context;

        public OrderService(InventoryContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders.ToListAsync();
        }

        public async Task AddOrderAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }
    }
}
