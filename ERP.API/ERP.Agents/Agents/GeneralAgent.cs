using ERP.API.ERP.Agents.Core;
using ERP.API.ERP.Agents.Prompts;
using ERP.API.ERP.Agents.Tools.Generals;
using ERP.API.Services;

namespace ERP.API.ERP.Agents.Agents
{
    public sealed class GeneralAgent
    {
        private readonly IOllamaService _ollamaService;
        private readonly GeneralPrompt _prompt;
        private readonly SearchKnowledgeTool _searchKnowledgeTool;

        public GeneralAgent(IOllamaService ollamaService, GeneralPrompt prompt, SearchKnowledgeTool searchKnowledgeTool)
        {
            _ollamaService = ollamaService;
            _prompt = prompt;
            _searchKnowledgeTool = searchKnowledgeTool;
        }

        public async Task<AgentRouteResult> HandleAsync(string query, CancellationToken cancellationToken = default)
        {
            var response = await _ollamaService.GenerateAsync(
                _prompt.GetInstructions(),
                query,
                _searchKnowledgeTool.ToAIFunction());

            return new AgentRouteResult("general", response, "general");
        }
    }
}
