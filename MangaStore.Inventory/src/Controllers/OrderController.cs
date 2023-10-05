using MangaStore.Inventory.Dtos;
using MangaStore.Inventory.Services;
using Microsoft.AspNetCore.Mvc;

namespace MangaStore.Inventory.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders() 
        { 
            var orders = await _orderService.GetAllOrdersAsync();

            return Ok(orders);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrder(CreateOrderDto createOrderDto)
        {
            var order = CreateOrderDto.MapToEntity(createOrderDto);
            await _orderService.AddOrderAsync(order);

            return CreatedAtAction(nameof(AddOrder), new { order.Id }, order);
        } 
    }
}
