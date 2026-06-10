using ERP.API.ERP.Agents.Agents;
using ERP.API.ERP.Agents.Prompts;
using ERP.API.Services;

namespace ERP.API.ERP.Agents.Core
{
    public sealed class RouterAgent
    {
        private readonly IOllamaService _ollamaService;
        private readonly GeneralAgent _generalAgent;
        private readonly FinanceAgent _financeAgent;
        private readonly EmailAgent _emailAgent;
        private readonly RouterPrompt _routerPrompt;

        public RouterAgent(
            IOllamaService ollamaService,
            GeneralAgent generalAgent,
            FinanceAgent financeAgent,
            EmailAgent emailAgent,
            RouterPrompt routerPrompt)
        {
            _ollamaService = ollamaService;
            _generalAgent = generalAgent;
            _financeAgent = financeAgent;
            _emailAgent = emailAgent;
            _routerPrompt = routerPrompt;
        }

        public async Task<AgentRouteResult> RouteAsync(string query, CancellationToken cancellationToken = default)
        {
            var classification = await _ollamaService.GenerateAsync(
                _routerPrompt.GetInstructions(),
                query);

            var route = AgentRoute.Parse(classification);
            return route.Name switch
            {
                AgentRouteNames.Finance => await _financeAgent.HandleAsync(query, cancellationToken),
                AgentRouteNames.Email => await _emailAgent.HandleAsync(query, cancellationToken),
                _ => await _generalAgent.HandleAsync(query, cancellationToken)
            };
        }
    }

    public sealed record AgentRouteResult(string Agent, string Response, string Intent);

    public static class AgentRouteNames
    {
        public const string General = "general";
        public const string Finance = "finance";
        public const string Email = "email";
    }

    public sealed record AgentRoute(string Name)
    {
        public static AgentRoute Parse(string value)
        {
            var normalized = value.Trim().ToLowerInvariant();
            if (normalized.Contains("finance") || normalized.Contains("invoice") || normalized.Contains("approval"))
            {
                return new AgentRoute(AgentRouteNames.Finance);
            }

            if (normalized.Contains("email") || normalized.Contains("notify") || normalized.Contains("send"))
            {
                return new AgentRoute(AgentRouteNames.Email);
            }

            return new AgentRoute(AgentRouteNames.General);
        }
    }
}
