using MangaStore.Inventory.Models;

namespace MangaStore.Inventory.Dtos
{
    public class CreateLocationDto
    {
        public Guid Id => Guid.NewGuid();
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }

        public static Location MapToEntity(CreateLocationDto createLocationDto)
        {
            return new Location
            {
                Id = createLocationDto.Id,
                Name = createLocationDto.Name,
                Address = createLocationDto.Address,
                PhoneNumber = createLocationDto.PhoneNumber,
            };
        }
    }
}
