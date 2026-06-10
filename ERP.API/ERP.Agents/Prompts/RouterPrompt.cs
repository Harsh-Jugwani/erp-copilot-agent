namespace ERP.API.ERP.Agents.Prompts
{
    public sealed class RouterPrompt
    {
        public string GetInstructions() => "Classify the query into exactly one intent: finance, email, or general. Return only the intent word.";
    }
}
