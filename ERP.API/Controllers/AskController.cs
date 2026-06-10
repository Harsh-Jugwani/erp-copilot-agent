using ERP.API.ERP.Agents.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class AskController : ControllerBase
    {
        private readonly AgentOrchestrator _orchestrator;

        public AskController(AgentOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] AskRequest request, CancellationToken cancellationToken)
        {
            var result = await _orchestrator.AskAsync(request.Query, cancellationToken);
            return Ok(result);
        }
    }

    public sealed record AskRequest(string Query);
}