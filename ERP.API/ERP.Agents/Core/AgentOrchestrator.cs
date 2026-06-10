using ERP.API.ERP.Agents.Guardrails;
using Microsoft.AspNetCore.Http;

namespace ERP.API.ERP.Agents.Core
{
    public sealed class AgentOrchestrator
    {
        private readonly RouterAgent _routerAgent;
        private readonly PromptInjectionGuardrail _promptInjectionGuardrail;
        private readonly ToolPermissionGuardrail _toolPermissionGuardrail;
        private readonly DataAccessGuardrail _dataAccessGuardrail;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AgentOrchestrator(
            RouterAgent routerAgent,
            PromptInjectionGuardrail promptInjectionGuardrail,
            ToolPermissionGuardrail toolPermissionGuardrail,
            DataAccessGuardrail dataAccessGuardrail,
            IHttpContextAccessor httpContextAccessor)
        {
            _routerAgent = routerAgent;
            _promptInjectionGuardrail = promptInjectionGuardrail;
            _toolPermissionGuardrail = toolPermissionGuardrail;
            _dataAccessGuardrail = dataAccessGuardrail;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AgentRouteResult> AskAsync(string query, CancellationToken cancellationToken = default)
        {
            _promptInjectionGuardrail.Validate(query);

            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
                ?? _httpContextAccessor.HttpContext?.User?.Identity?.Name
                ?? string.Empty;

            _toolPermissionGuardrail.Validate(userId, query);
            _dataAccessGuardrail.ValidateUserContext(userId);

            return await _routerAgent.RouteAsync(query, cancellationToken);
        }
    }
}
