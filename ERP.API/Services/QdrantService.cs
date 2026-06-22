using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Net.Http;
using System.Text.Json;
using System.Linq;
using ERP.API.Configurations;

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
        private readonly QdrantConfig _config;
        private string _collectionName = "documents";
        private readonly IEmbeddingService _embeddingService;
        private readonly HttpClient _httpClient;

        public QdrantService(QdrantClient client, QdrantConfig config, HttpClient httpClient, IEmbeddingService embeddingService)
        {
            _client = client;
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));

            if (!string.IsNullOrWhiteSpace(_config.CollectionName))
                _collectionName = _config.CollectionName!;
        }

        public async Task<List<string>> SearchAsync(string query, ulong limit = 5)
        {
            var vector = await _embeddingService.GenerateEmbeddingAsync(query);

            try
            {
                var result = await _client.SearchAsync(_collectionName, vector, limit: limit);
                return result.Select(r => r.Payload.TryGetValue("text", out var value) ? value?.StringValue ?? string.Empty : string.Empty).ToList();
            }
            catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Unimplemented || ex.Message.Contains("404"))
            {
                // Fallback to REST HTTP API when gRPC not available
                var scheme = _config.Https ? "https" : "http";
                var baseUrl = $"{scheme}://{_config.Host}:{_config.Port}";
                var requestUri = $"{baseUrl}/collections/{_collectionName}/points/search";

                var requestBody = new
                {
                    vector = vector.ToArray(),
                    limit = limit,
                    with_payload = true
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(requestUri, content);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);

                var results = new List<string>();
                if (doc.RootElement.TryGetProperty("result", out var resultArray) && resultArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in resultArray.EnumerateArray())
                    {
                        if (item.TryGetProperty("payload", out var payload) && payload.TryGetProperty("text", out var textProp))
                        {
                            results.Add(textProp.GetString() ?? string.Empty);
                        }
                    }
                }

                return results;
            }
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
