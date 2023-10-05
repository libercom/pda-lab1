using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MangaStore.Catalog.Config;
using MangaStore.Catalog.Models;

namespace MangaStore.Catalog.Services
{
    public class CatalogService
    {
        private readonly IMongoCollection<Manga> _mangasCollection;

        public CatalogService(IOptions<CatalogDatabaseSettings> catalogDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                catalogDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                catalogDatabaseSettings.Value.DatabaseName);

            _mangasCollection = mongoDatabase.GetCollection<Manga>(
                catalogDatabaseSettings.Value.MangasCollectionName);
        }

        public async Task<List<Manga>> GetAllAsync() =>
            await _mangasCollection.Find(_ => true).ToListAsync();

        public async Task<Manga?> GetByIdAsync(Guid id) =>
            await _mangasCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Manga newManga) =>
            await _mangasCollection.InsertOneAsync(newManga);

        public async Task UpdateAsync(Guid id, Manga updatedManga) =>
            await _mangasCollection.ReplaceOneAsync(x => x.Id == id, updatedManga);

        public async Task RemoveAsync(Guid id) =>
            await _mangasCollection.DeleteOneAsync(x => x.Id == id);
    }
}
