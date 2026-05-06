using System.Text.Json.Serialization;

namespace MiniMax.Core;

public class BaseResp
{
    [JsonPropertyName("status_code")]
    public int StatusCode { get; set; }

    [JsonPropertyName("status_msg")]
    public string? StatusMsg { get; set; }
}
