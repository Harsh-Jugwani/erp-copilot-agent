namespace ERP.Agents.Guardrails
{
    public sealed class ToolPermissionGuardrail
    {
        public void Validate(string userId, string query)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException("Authenticated user is required for tool access.");
            }

            if (query.Contains("delete", StringComparison.OrdinalIgnoreCase) || query.Contains("drop table", StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Destructive actions are not allowed through the AI layer.");
            }
        }
    }
}
