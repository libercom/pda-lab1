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

        public async Task<Order> AddOrderAsync(Order order)
        {
            var mangaStock = _context.MangaStocks.FirstOrDefault(x => x.MangaId == order.MangaId);
            
            if (mangaStock == null)
            {
                return null;
            }

            if (mangaStock.Quantity < order.Quantity)
            {
                return null;
            }

            mangaStock.Quantity -= order.Quantity;
            _context.Orders.Add(order);
            
            await _context.SaveChangesAsync();

            return order;
        }
    }
}
