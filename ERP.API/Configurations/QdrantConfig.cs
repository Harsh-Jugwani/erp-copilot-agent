namespace ERP.API.Configurations
{
    public class QdrantConfig    {
        public string? Host { get; set; }
        public int Port { get; set; }
        public bool  Https { get; set; }
        public string? ApiKey { get; set; }
        public string? CollectionName { get; set; }
        public int VectorSize { get; set; }
    }
}
