namespace ERP.Agents.Prompts
{
    public sealed class RouterPrompt
    {
        public string GetInstructions() => @"Classify the user's query into exactly one intent: finance, email, or general.
Return ONLY the single intent word (finance, email, or general) and nothing else.

Rules:
- If the user explicitly asks to send or notify via email (phrases like 'via email', 'send email', 'email', 'notify', 'e-mail', 'mail'), choose: email
- If the user requests approval for a specific invoice (mentions 'approval' or 'approve' together with an invoice identifier such as INV-2026-002 or a numeric invoice id), choose: email
- If the query is about invoices, payments, balances, approvers, or other financial operations but does NOT request sending via email or approval-of-specific-invoice, choose: finance
- Otherwise choose: general

Examples:
""Request approval for invoice INV-2026-002 via email"" => email
""Please approve invoice INV-2026-002"" => email
""What is the status of invoice 12345?"" => finance
""List pending invoices for approval"" => finance
""Remind the team about the meeting"" => general";
    }
}
