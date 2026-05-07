using System.Text.Json.Serialization;
using static MiniMax.Models.Enums;

namespace MiniMax.Models.Video;

public class VideoGenerationReq
{
    [JsonPropertyName("model")]
    public VideoModel Model { get; set; } = VideoModel.MiniMaxHailuo23;

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("prompt_optimizer")]
    public bool PromptOptimizer { get; set; } = true;

    [JsonPropertyName("fast_pretreatment")]
    public bool FastPretreatment { get; set; } = false;

    [JsonPropertyName("duration")]
    public int Duration { get; set; } = 6;

    [JsonPropertyName("resolution")]
    public VideoResolution Resolution { get; set; } = VideoResolution.P768;

    [JsonPropertyName("first_frame_image")]
    public string? FirstFrameImage { get; set; }

    [JsonPropertyName("last_frame_image")]
    public string? LastFrameImage { get; set; }

    [JsonPropertyName("subject_reference")]
    public List<SubjectReference>? SubjectReference { get; set; }

    [JsonPropertyName("callback_url")]
    public string? CallbackUrl { get; set; }

    [JsonPropertyName("aigc_watermark")]
    public bool AigcWatermark { get; set; } = false;
}

public class SubjectReference
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "character";

    [JsonPropertyName("image")]
    public List<string> Image { get; set; } = new();
}

public class VideoGenerationResp
{
    [JsonPropertyName("task_id")]
    public string TaskId { get; set; } = string.Empty;

    [JsonPropertyName("base_resp")]
    public Core.BaseResp? BaseResp { get; set; }
}

public class VideoQueryResp
{
    [JsonPropertyName("task_id")]
    public string TaskId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public VideoProcessStatus Status { get; set; } = VideoProcessStatus.Preparing;

    [JsonPropertyName("file_id")]
    public string? FileId { get; set; }

    [JsonPropertyName("video_width")]
    public int VideoWidth { get; set; }

    [JsonPropertyName("video_height")]
    public int VideoHeight { get; set; }

    [JsonPropertyName("video_info")]
    public VideoInfo? VideoInfo { get; set; }

    [JsonPropertyName("base_resp")]
    public Core.BaseResp? BaseResp { get; set; }
}

public class VideoInfo
{
    [JsonPropertyName("video_url")]
    public string? VideoUrl { get; set; }
}
