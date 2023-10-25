using MangaStore.Inventory.Dtos;
using MangaStore.Inventory.Models;
using MangaStore.Inventory.Services;
using Microsoft.AspNetCore.Mvc;

namespace MangaStore.Inventory.Controllers
{
    [ApiController]
    [Route("api/stocks")]
    public class MangaStockController : ControllerBase
    {
        private readonly MangaStockService _mangaStockService;

        public MangaStockController(MangaStockService mangaStockService)
        {
            _mangaStockService = mangaStockService;
        }

        [HttpGet("{mangaId}")]
        public async Task<IActionResult> GetByMangaId(Guid mangaId)
        {
            var mangaStocks = await _mangaStockService.GetByMangaIdAsync(mangaId);

            return Ok(mangaStocks);
        }

        [HttpPost]
        public async Task<IActionResult> AddMangaStock(CreateMangaStockDto createMangaStockDto)
        {
            var mangaStock = CreateMangaStockDto.MapToEntity(createMangaStockDto);
            await _mangaStockService.AddMangaStockAsync(mangaStock);

            return CreatedAtAction(nameof(AddMangaStock), 
                new { createMangaStockDto.MangaId, createMangaStockDto.LocationId }, mangaStock);
        }

        [HttpPost("sync")]
        public async Task<IActionResult> SyncMangaStock(SyncMangaStockDto syncMangaStockDto)
        {
            await _mangaStockService.SyncMangaStockAsync(syncMangaStockDto.MangaId);

            return NoContent();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateMangaStock(MangaStock mangaStock)
        {
            await _mangaStockService.UpdateMangaStockAsync(mangaStock);

            return NoContent();
        }
    }
}
