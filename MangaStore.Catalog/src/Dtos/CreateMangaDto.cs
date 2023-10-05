using MangaStore.Catalog.Models;

namespace MangaStore.Catalog.Dtos
{
    public class CreateMangaDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }

        public static Manga MapToEntity(CreateMangaDto createMangaDto)
        {
            return new Manga
            {
                Id = Guid.NewGuid(),
                Title = createMangaDto.Title,
                Description = createMangaDto.Description,
                Category = createMangaDto.Category,
                Author = createMangaDto.Author,
                Price = createMangaDto.Price,
            };
        }
    }
}
