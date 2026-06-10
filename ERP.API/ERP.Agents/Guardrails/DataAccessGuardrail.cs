namespace ERP.API.ERP.Agents.Guardrails
{
    public sealed class DataAccessGuardrail
    {
        public void ValidateUserContext(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException("Unable to resolve the current user context.");
            }
        }
    }
}
