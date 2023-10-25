using MangaStore.Inventory.Dtos;
using MangaStore.Inventory.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace MangaStore.Inventory.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _catalogUrl;

        public OrderController(OrderService orderService, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _orderService = orderService;
            _httpClientFactory = httpClientFactory;
            _catalogUrl = configuration["CatalogServiceUrl"];
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
            HttpClient httpClient = _httpClientFactory.CreateClient();
            MangaDto? mangaDto = await httpClient.GetFromJsonAsync<MangaDto>(_catalogUrl + $"/{createOrderDto.MangaId}");

            if (mangaDto == null)
            {
                return StatusCode(500, "Internal Server Error");
            }

            var order = CreateOrderDto.MapToEntity(createOrderDto, mangaDto!.Price);
            var result = await _orderService.AddOrderAsync(order);

            if (result == null)
            {
                return BadRequest("Manga not available at the moment");
            }

            return CreatedAtAction(nameof(AddOrder), new { order.Id }, order);
        } 
    }
}
