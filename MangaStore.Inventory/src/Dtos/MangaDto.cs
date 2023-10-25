namespace MangaStore.Inventory.Dtos
{
    public class MangaDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }
    }
}
