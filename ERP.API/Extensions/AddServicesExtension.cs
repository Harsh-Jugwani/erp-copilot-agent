using ERP.API.Configurations;
using ERP.API.ERP.Agents.Agents;
using ERP.API.ERP.Agents.Core;
using ERP.API.ERP.Agents.Guardrails;
using ERP.API.ERP.Agents.Prompts;
using ERP.API.ERP.Agents.Tools.Emails;
using ERP.API.ERP.Agents.Tools.Finance;
using ERP.API.ERP.Agents.Tools.Generals;
using ERP.API.Services;
using Microsoft.Extensions.AI;
using Qdrant.Client;

namespace ERP.API.Extensions
{
    public static class AddServicesExtension
    {
        public static void AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            var ollamaConfig = configuration.GetSection("OllamaConfig").Get<OllamaConfig>() ?? throw new InvalidOperationException("OllamaConfig is missing.");
            var qdrantConfig = configuration.GetSection("QdrantConfig").Get<QdrantConfig>() ?? throw new InvalidOperationException("QdrantConfig is missing.");
            var httpClient = new HttpClient();

            services.AddScoped<ISeedDataService, SeedDataService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IOllamaService, OllamaService>();
            services.AddScoped<IEmbeddingService, EmbeddingService>();
            services.AddScoped<IQdrantService, QdrantService>();
            services.AddScoped<RouterAgent>();
            services.AddScoped<AgentOrchestrator>();
            services.AddScoped<GeneralAgent>();
            services.AddScoped<FinanceAgent>();
            services.AddScoped<EmailAgent>();
            services.AddScoped<RouterPrompt>();
            services.AddScoped<GeneralPrompt>();
            services.AddScoped<FinancePrompt>();
            services.AddScoped<EmailPrompt>();
            services.AddScoped<PromptInjectionGuardrail>();
            services.AddScoped<ToolPermissionGuardrail>();
            services.AddScoped<DataAccessGuardrail>();
            services.AddScoped<SearchKnowledgeTool>();
            services.AddScoped<GetInvoiceDetailsTool>();
            services.AddScoped<GetPendingInvoicesTool>();
            services.AddScoped<GetApproversTool>();
            services.AddScoped<SendApprovalEmailTool>();

            services.AddSingleton(httpClient);
            services.AddSingleton<IChatClient>(_ => new OllamaChatClient(new Uri(ollamaConfig.Endpoint!), ollamaConfig.ChatModel!, httpClient));
            services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(_ => new OllamaEmbeddingGenerator(new Uri(ollamaConfig.Endpoint!), ollamaConfig.EmbeddingModel!, httpClient));
            services.AddSingleton(new QdrantClient(
                host: qdrantConfig.Host!,
                port: qdrantConfig.Port,
                https: qdrantConfig.Https,
                apiKey: qdrantConfig.ApiKey));
        }
    }
}
