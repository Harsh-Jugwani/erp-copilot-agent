namespace ERP.API.ERP.Agents.Prompts
{
    public sealed class EmailPrompt
    {
        public string GetInstructions() => "You are an ERP email assistant. Prepare compliant approval-related emails only when the action is authorized.";
    }
}
