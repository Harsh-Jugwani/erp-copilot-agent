using ERP.API.Services;
using Microsoft.Extensions.AI;

namespace ERP.API.ERP.Agents.Tools.Generals
{
    public sealed class SearchKnowledgeTool
    {
        private readonly IQdrantService _qdrantService;

        public SearchKnowledgeTool(IQdrantService qdrantService)
        {
            _qdrantService = qdrantService;
        }

        public AIFunction ToAIFunction() => AIFunctionFactory.Create((Func<string, int, Task<List<string>>>)SearchAsync, nameof(SearchKnowledgeTool), "Search approved ERP knowledge and memory.", null);

        public Task<List<string>> SearchAsync(string query, int limit = 5) => _qdrantService.SearchAsync(query, (ulong)Math.Max(limit, 1));
    }
}
