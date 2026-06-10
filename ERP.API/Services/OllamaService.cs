using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;

namespace ERP.API.Services;

public interface IOllamaService
{
    Task<string> GenerateAsync(
        string instructions,
        string query,
        params AIFunction[] tools);
    IAsyncEnumerable<string> GenerateStreamingAsync(
        string instructions,
        string query,
        params AIFunction[] tools);
}

public sealed class OllamaService : IOllamaService
{
    private readonly IChatClient _chatClient;
    public OllamaService(IChatClient chatClient)
    {
        _chatClient = new FunctionInvokingChatClient(chatClient);
    }

    public async Task<string> GenerateAsync(
        string instructions,
        string query,
        params AIFunction[] tools)
    {
        var options = new ChatOptions { Instructions = instructions, Tools = tools };
        var response = await _chatClient.GetResponseAsync([new ChatMessage(ChatRole.User, query)], options);
        return response.Text ?? string.Empty;
    }

    public async IAsyncEnumerable<string> GenerateStreamingAsync(
        string instructions,
        string query,
        params AIFunction[] tools)
    {
        var options = new ChatOptions { Instructions = instructions, Tools = tools };
        await foreach (var update in _chatClient.GetStreamingResponseAsync([new ChatMessage(ChatRole.User, query)], options))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                yield return update.Text;
            }
        }
    }
}