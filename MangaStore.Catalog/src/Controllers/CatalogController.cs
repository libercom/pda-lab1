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

        public CatalogController(CatalogService catalogService)
        {
            _catalogService = catalogService;
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
