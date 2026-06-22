using System.Text.RegularExpressions;

namespace ERP.Agents.Guardrails
{
    public sealed class PromptInjectionGuardrail
    {
        private static readonly Regex SuspiciousPattern = new(@"(ignore\s+previous|system\s+prompt|developer\s+message|reveal\s+secrets|exfiltrat|jailbreak)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public void Validate(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new InvalidOperationException("Query is required.");
            }

            if (SuspiciousPattern.IsMatch(query))
            {
                throw new UnauthorizedAccessException("Potential prompt injection detected.");
            }
        }
    }
}
