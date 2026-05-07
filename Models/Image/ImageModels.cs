using System.Text.Json.Serialization;
using static MiniMax.Models.Enums;

namespace MiniMax.Models.Image;

public class ImageGenerationReq
{
    [JsonPropertyName("model")]
    public ImageModel Model { get; set; } = ImageModel.Image01;

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("subject_reference")]
    public List<ImageSubjectReference>? SubjectReference { get; set; }

    [JsonPropertyName("style")]
    public StyleObject? Style { get; set; }

    [JsonPropertyName("aspect_ratio")]
    public ImageAspectRatio? AspectRatio { get; set; }

    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("height")]
    public int? Height { get; set; }

    [JsonPropertyName("response_format")]
    public ImageResponseFormat ResponseFormat { get; set; } = ImageResponseFormat.Url;

    [JsonPropertyName("seed")]
    public long? Seed { get; set; }

    [JsonPropertyName("n")]
    public int N { get; set; } = 1;

    [JsonPropertyName("prompt_optimizer")]
    public bool PromptOptimizer { get; set; } = false;

    [JsonPropertyName("aigc_watermark")]
    public bool AigcWatermark { get; set; } = false;
}

public class ImageSubjectReference
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "character";

    [JsonPropertyName("image_file")]
    public string ImageFile { get; set; } = string.Empty;
}

public class StyleObject
{
    [JsonPropertyName("style_type")]
    public string StyleType { get; set; } = string.Empty;

    [JsonPropertyName("style_weight")]
    public float StyleWeight { get; set; } = 0.8f;
}

public class ImageGenerationResp
{
    [JsonPropertyName("data")]
    public ImageData? Data { get; set; }

    [JsonPropertyName("metadata")]
    public ImageMetadata? Metadata { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("base_resp")]
    public Core.BaseResp? BaseResp { get; set; }
}

public class ImageData
{
    [JsonPropertyName("image_urls")]
    public List<string>? ImageUrls { get; set; }

    [JsonPropertyName("image_base64")]
    public List<string>? ImageBase64 { get; set; }
}

public class ImageMetadata
{
    [JsonPropertyName("success_count")]
    public int SuccessCount { get; set; }

    [JsonPropertyName("failed_count")]
    public int FailedCount { get; set; }
}
