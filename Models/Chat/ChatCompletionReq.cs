using System.Text.Json.Serialization;

namespace MiniMax.Models.Chat;

public class ChatCompletionReq
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "MiniMax-M2.7";

    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;

    [JsonPropertyName("max_completion_tokens")]
    public long? MaxCompletionTokens { get; set; }

    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }

    [JsonPropertyName("messages")]
    public List<Message> Messages { get; set; } = new();
}

public class Message
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}
