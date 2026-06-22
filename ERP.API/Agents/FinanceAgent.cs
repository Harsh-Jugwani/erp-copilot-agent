using ERP.Agents.Core;
using ERP.Agents.Prompts;
using ERP.Agents.Tools.Finance;
using ERP.API.Services;

namespace ERP.API.Agents
{
    public sealed class FinanceAgent
    {
        private readonly IOllamaService _ollamaService;
        private readonly FinancePrompt _prompt;
        private readonly GetInvoiceDetailsTool _getInvoiceDetailsTool;
        private readonly GetPendingInvoicesTool _getPendingInvoicesTool;
        private readonly GetApproversTool _getApproversTool;

        public FinanceAgent(
            IOllamaService ollamaService,
            FinancePrompt prompt,
            GetInvoiceDetailsTool getInvoiceDetailsTool,
            GetPendingInvoicesTool getPendingInvoicesTool,
            GetApproversTool getApproversTool)
        {
            _ollamaService = ollamaService;
            _prompt = prompt;
            _getInvoiceDetailsTool = getInvoiceDetailsTool;
            _getPendingInvoicesTool = getPendingInvoicesTool;
            _getApproversTool = getApproversTool;
        }

        public async Task<AgentRouteResult> HandleAsync(string query, CancellationToken cancellationToken = default)
        {
            var tools = new[]
            {
                _getInvoiceDetailsTool.ToAIFunction(),
                _getPendingInvoicesTool.ToAIFunction(),
                _getApproversTool.ToAIFunction()
            };

            var response = await _ollamaService.GenerateAsync(
                _prompt.GetInstructions(),
                query,
                tools);

            return new AgentRouteResult("finance", response, "finance");
        }
    }
}
