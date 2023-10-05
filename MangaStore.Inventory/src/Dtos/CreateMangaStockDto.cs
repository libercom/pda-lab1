using MangaStore.Inventory.Models;

namespace MangaStore.Inventory.Dtos
{
    public class CreateMangaStockDto
    {
        public Guid MangaId { get; set; }
        public Guid LocationId { get; set; }
        public int Quantity { get; set; }
        public DateTimeOffset RestockDate { get; set; }

        public static MangaStock MapToEntity(CreateMangaStockDto createMangaStockDto)
        {
            return new MangaStock
            {
                MangaId = createMangaStockDto.MangaId,
                LocationId = createMangaStockDto.LocationId,
                Quantity = createMangaStockDto.Quantity,
                RestockDate = createMangaStockDto.RestockDate
            };
        }
    }
}
