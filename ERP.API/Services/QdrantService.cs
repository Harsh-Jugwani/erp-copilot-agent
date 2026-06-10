using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace ERP.API.Services
{
    public interface IQdrantService
    {
        Task UpsertAsync(
            string id,
            string content,
            Dictionary<string, object>? metadata = null);

        Task<List<string>> SearchAsync(
            string query,
            ulong limit = 5);
    }

    public sealed class QdrantService : IQdrantService
    {
        private readonly QdrantClient _client;
        private readonly string _collectionName = "documents";
        private readonly IEmbeddingService _embeddingService;

        public QdrantService(QdrantClient client, IEmbeddingService embeddingService)
        {
            _client = client;
            _embeddingService = embeddingService;
        }

        public async Task<List<string>> SearchAsync(string query, ulong limit = 5)
        {
            var vector = await _embeddingService.GenerateEmbeddingAsync(query);
            var result = await _client.SearchAsync(_collectionName, vector, limit: limit);
            return result.Select(r => r.Payload.TryGetValue("text", out var value) ? value?.StringValue ?? string.Empty : string.Empty).ToList();
        }

        public async Task UpsertAsync(string id, string content, Dictionary<string, object>? metadata = null)
        {
            var vector = await _embeddingService.GenerateEmbeddingAsync(content);
            var payload = new Dictionary<string, Qdrant.Client.Grpc.Value>
            {
                ["text"] = new Qdrant.Client.Grpc.Value { StringValue = content },
                ["documentId"] = new Qdrant.Client.Grpc.Value { StringValue = id }
            };

            if (metadata is not null)
            {
                foreach (var item in metadata)
                {
                    payload[item.Key] = new Qdrant.Client.Grpc.Value { StringValue = item.Value?.ToString() ?? string.Empty };
                }
            }

            await _client.UpsertAsync(
                collectionName: _collectionName,
                points: new[]
                {
                    new PointStruct
                    {
                        Id = Guid.NewGuid(),
                        Vectors = vector.ToArray(),
                        Payload = { payload }
                    }
                });
        }
    }
}
