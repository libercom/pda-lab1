namespace MangaStore.Inventory.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public Guid MangaId { get; set; }
        public Guid LocationId { get; set; }
        public MangaStock MangaStock { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTimeOffset PickupDate { get; set; }
    }
}
