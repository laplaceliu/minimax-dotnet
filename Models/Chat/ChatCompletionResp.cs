using System.Text.Json.Serialization;
using MiniMax.Core;

namespace MiniMax.Models.Chat;

public class ChatCompletionResp
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("choices")]
    public List<Choice> Choices { get; set; } = new();

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("usage")]
    public Usage? Usage { get; set; }

    [JsonPropertyName("input_sensitive")]
    public bool InputSensitive { get; set; }

    [JsonPropertyName("input_sensitive_type")]
    public int? InputSensitiveType { get; set; }

    [JsonPropertyName("output_sensitive")]
    public bool OutputSensitive { get; set; }

    [JsonPropertyName("output_sensitive_type")]
    public int? OutputSensitiveType { get; set; }

    [JsonPropertyName("output_sensitive_int")]
    public int? OutputSensitiveInt { get; set; }

    [JsonPropertyName("base_resp")]
    public BaseResp? BaseResp { get; set; }
}

public class Choice
{
    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public Message? Message { get; set; }

    [JsonPropertyName("delta")]
    public Message? Delta { get; set; }
}

public class Usage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }

    [JsonPropertyName("total_characters")]
    public int TotalCharacters { get; set; }

    [JsonPropertyName("prompt_tokens_details")]
    public PromptTokensDetails? PromptTokensDetails { get; set; }
}

public class PromptTokensDetails
{
    [JsonPropertyName("cached_tokens")]
    public int CachedTokens { get; set; }
}
