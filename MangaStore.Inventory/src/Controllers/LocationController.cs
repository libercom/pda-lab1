using MangaStore.Inventory.Dtos;
using MangaStore.Inventory.Services;
using Microsoft.AspNetCore.Mvc;

namespace MangaStore.Inventory.Controllers
{
    [ApiController]
    [Route("api/locations")]
    public class LocationController : ControllerBase
    {
        private readonly LocationService _locationService;

        public LocationController(LocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLocations()
        {
            var locations = await _locationService.GetAllLocationsAsync();

            return Ok(locations);
        }

        [HttpPost]
        public async Task<IActionResult> AddLocation(CreateLocationDto createLocationDto)
        {
            var location = CreateLocationDto.MapToEntity(createLocationDto);
            await _locationService.AddLocationAsync(location);

            return CreatedAtAction(nameof(AddLocation), new { location.Id }, location);
        }
    }
}
