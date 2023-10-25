using MangaStore.Inventory.Models;

namespace MangaStore.Inventory.Dtos
{
    public class CreateOrderDto
    {
        public Guid Id => Guid.NewGuid();
        public Guid MangaId { get; set; }
        public Guid LocationId { get; set; }
        public int Quantity { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTimeOffset PickupDate { get; set; }

        public static Order MapToEntity(CreateOrderDto createOrderDto, decimal price)
        {
            return new Order
            {
                Id = createOrderDto.Id,
                MangaId = createOrderDto.MangaId,
                LocationId = createOrderDto.LocationId,
                Quantity = createOrderDto.Quantity,
                FullName = createOrderDto.FullName,
                PhoneNumber = createOrderDto.PhoneNumber,
                PickupDate = createOrderDto.PickupDate,
                Price = price
            };
        }
    }
}
