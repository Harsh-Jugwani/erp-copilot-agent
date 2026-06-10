using Microsoft.Extensions.AI;

namespace ERP.API.Services
{
    public interface IEmbeddingService
    {
        Task<float[]> GenerateEmbeddingAsync(string text);
        Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(
        IEnumerable<string> texts);
    }
    public class EmbeddingService : IEmbeddingService
    {
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
        public EmbeddingService(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
        {
            _embeddingGenerator = embeddingGenerator;
        }
        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            var embedding = await _embeddingGenerator.GenerateAsync(text);

            return embedding.Vector.ToArray();
        }
        public async Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(
       IEnumerable<string> texts)
        {
            var embeddings = await _embeddingGenerator.GenerateAsync(texts);

            return embeddings
                .Select(x => x.Vector.ToArray())
                .ToList();
        }
    }
}
