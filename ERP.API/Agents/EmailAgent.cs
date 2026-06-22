using ERP.Agents.Prompts;
using ERP.Agents.Core;
using ERP.Agents.Tools.Emails;
using ERP.API.Services;

namespace ERP.API.Agents
{
    public sealed class EmailAgent
    {
        private readonly IOllamaService _ollamaService;
        private readonly EmailPrompt _prompt;
        private readonly SendApprovalEmailTool _sendApprovalEmailTool;

        public EmailAgent(IOllamaService ollamaService, EmailPrompt prompt, SendApprovalEmailTool sendApprovalEmailTool)
        {
            _ollamaService = ollamaService;
            _prompt = prompt;
            _sendApprovalEmailTool = sendApprovalEmailTool;
        }

        public async Task<AgentRouteResult> HandleAsync(string query, CancellationToken cancellationToken = default)
        {
            var response = await _ollamaService.GenerateAsync(
                _prompt.GetInstructions(),
                query,
                tools : [_sendApprovalEmailTool.ToAIFunction()]);

            return new AgentRouteResult("email", response, "email");
        }
    }
}
