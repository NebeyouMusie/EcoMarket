namespace EcoMarket.Models
{
    public class DatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string UserCollectionName { get; set; } = null!;
        public string ProductCollectionName { get; set; } = null!;
        public string OrderCollectionName { get; set; } = null!;
        public string ReviewCollectionName { get; set; } = null!;
        public string FavoriteCollectionName { get; set; } = null!;
    }
}
