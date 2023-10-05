namespace MangaStore.Inventory.Models
{
    public class MangaStock
    {
        public Guid MangaId { get; set; }
        public Guid LocationId { get; set; }
        public Location Location { get; set; }
        public int Quantity { get; set; }
        public DateTimeOffset RestockDate { get; set; }
    }
}
