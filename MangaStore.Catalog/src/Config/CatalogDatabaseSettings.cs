namespace MangaStore.Catalog.Config
{
    public class CatalogDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string MangasCollectionName { get; set; } = null!;
    }
}
