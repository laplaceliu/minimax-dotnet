using System.Text.Json.Serialization;
using static MiniMax.Models.Enums;

namespace MiniMax.Models.Voice;

public class VoiceCloneReq
{
    [JsonPropertyName("file_id")]
    public long FileId { get; set; }

    [JsonPropertyName("voice_id")]
    public string VoiceId { get; set; } = string.Empty;

    [JsonPropertyName("clone_prompt")]
    public ClonePrompt? ClonePrompt { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("model")]
    public SpeechModel? Model { get; set; }

    [JsonPropertyName("language_boost")]
    public LanguageBoost? LanguageBoost { get; set; }

    [JsonPropertyName("need_noise_reduction")]
    public bool NeedNoiseReduction { get; set; } = false;

    [JsonPropertyName("need_volume_normalization")]
    public bool NeedVolumeNormalization { get; set; } = false;

    [JsonPropertyName("aigc_watermark")]
    public bool AigcWatermark { get; set; } = false;
}

public class ClonePrompt
{
    [JsonPropertyName("prompt_audio")]
    public long PromptAudio { get; set; }

    [JsonPropertyName("prompt_text")]
    public string PromptText { get; set; } = string.Empty;
}

public class VoiceCloneResp
{
    [JsonPropertyName("input_sensitive")]
    public bool InputSensitive { get; set; }

    [JsonPropertyName("input_sensitive_type")]
    public int? InputSensitiveType { get; set; }

    [JsonPropertyName("demo_audio")]
    public string? DemoAudio { get; set; }

    [JsonPropertyName("base_resp")]
    public Core.BaseResp? BaseResp { get; set; }
}

public class GetVoiceReq
{
    [JsonPropertyName("voice_type")]
    public VoiceType VoiceType { get; set; } = VoiceType.All;
}

public class GetVoiceResp
{
    [JsonPropertyName("system_voice")]
    public List<SystemVoiceInfo>? SystemVoice { get; set; }

    [JsonPropertyName("voice_cloning")]
    public List<VoiceCloningInfo>? VoiceCloning { get; set; }

    [JsonPropertyName("voice_generation")]
    public List<VoiceGenerationInfo>? VoiceGeneration { get; set; }

    [JsonPropertyName("base_resp")]
    public Core.BaseResp? BaseResp { get; set; }
}

public class SystemVoiceInfo
{
    [JsonPropertyName("voice_id")]
    public string VoiceId { get; set; } = string.Empty;

    [JsonPropertyName("voice_name")]
    public string VoiceName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public List<string>? Description { get; set; }
}

public class VoiceCloningInfo
{
    [JsonPropertyName("voice_id")]
    public string VoiceId { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public List<string>? Description { get; set; }

    [JsonPropertyName("created_time")]
    public string CreatedTime { get; set; } = string.Empty;
}

public class VoiceGenerationInfo
{
    [JsonPropertyName("voice_id")]
    public string VoiceId { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public List<string>? Description { get; set; }

    [JsonPropertyName("created_time")]
    public string CreatedTime { get; set; } = string.Empty;
}

public class DeleteVoiceReq
{
    [JsonPropertyName("voice_id")]
    public string VoiceId { get; set; } = string.Empty;

    [JsonPropertyName("voice_type")]
    public VoiceType VoiceType { get; set; } = VoiceType.All;
}

public class DeleteVoiceResp
{
    [JsonPropertyName("base_resp")]
    public Core.BaseResp? BaseResp { get; set; }
}

public class VoiceDesignReq
{
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("preview_text")]
    public string PreviewText { get; set; } = string.Empty;

    [JsonPropertyName("voice_id")]
    public string? VoiceId { get; set; }

    [JsonPropertyName("aigc_watermark")]
    public bool AigcWatermark { get; set; } = false;
}

public class VoiceDesignResp
{
    [JsonPropertyName("voice_id")]
    public string VoiceId { get; set; } = string.Empty;

    [JsonPropertyName("trial_audio")]
    public string? TrialAudio { get; set; }

    [JsonPropertyName("base_resp")]
    public Core.BaseResp? BaseResp { get; set; }
}
