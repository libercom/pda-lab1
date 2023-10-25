using Microsoft.AspNetCore.Mvc;
using MangaStore.Catalog.Dtos;
using MangaStore.Catalog.Services;

namespace MangaStore.Catalog.Controllers
{
    [ApiController]
    [Route("api/mangas")]
    public class CatalogController : ControllerBase
    {
        private readonly CatalogService _catalogService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _inventoryUrl;
        public CatalogController(CatalogService catalogService, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _catalogService = catalogService;
            _httpClientFactory = httpClientFactory;
            _inventoryUrl = configuration["InventoryServiceUrl"];
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMangas()
        {
            var mangas = await _catalogService.GetAllAsync();

            return Ok(mangas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMangaById(Guid id)
        {
            var manga = await _catalogService.GetByIdAsync(id);

            return Ok(manga);
        }

        [HttpPost]
        public async Task<IActionResult> CreateManga(CreateMangaDto createMangaDto)
        {
            var manga = CreateMangaDto.MapToEntity(createMangaDto);

            await _catalogService.CreateAsync(manga);

            HttpClient httpClient = _httpClientFactory.CreateClient();
            HttpResponseMessage response = await httpClient.PostAsJsonAsync(_inventoryUrl, new SyncMangaStockDto { MangaId = manga.Id });
            
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(500, "Internal Server Error");
            }

            return CreatedAtAction(nameof(CreateManga), new { id = manga.Id }, manga);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteManga(Guid id)
        {
            await _catalogService.RemoveAsync(id);

            return NoContent();
        }
    }
}
