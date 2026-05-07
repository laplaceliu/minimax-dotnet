using System.Text.Json.Serialization;
using MiniMax.Core;

namespace MiniMax.Models.Speech;

public class T2aV2Resp
{
    [JsonPropertyName("data")]
    public T2aV2Data? Data { get; set; }

    [JsonPropertyName("trace_id")]
    public string? TraceId { get; set; }

    [JsonPropertyName("extra_info")]
    public T2aV2ExtraInfo? ExtraInfo { get; set; }

    [JsonPropertyName("base_resp")]
    public BaseResp? BaseResp { get; set; }
}

public class T2aV2Data
{
    [JsonPropertyName("audio")]
    public string? Audio { get; set; }

    [JsonPropertyName("subtitle_file")]
    public string? SubtitleFile { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }
}

public class T2aV2ExtraInfo
{
    [JsonPropertyName("audio_length")]
    public long AudioLength { get; set; }

    [JsonPropertyName("audio_sample_rate")]
    public int AudioSampleRate { get; set; }

    [JsonPropertyName("audio_size")]
    public long AudioSize { get; set; }

    [JsonPropertyName("bitrate")]
    public int Bitrate { get; set; }

    [JsonPropertyName("audio_format")]
    public string AudioFormat { get; set; } = string.Empty;

    [JsonPropertyName("audio_channel")]
    public int AudioChannel { get; set; }

    [JsonPropertyName("invisible_character_ratio")]
    public float InvisibleCharacterRatio { get; set; }

    [JsonPropertyName("usage_characters")]
    public int UsageCharacters { get; set; }

    [JsonPropertyName("word_count")]
    public int WordCount { get; set; }
}
