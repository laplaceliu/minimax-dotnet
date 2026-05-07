using System.Text.Json.Serialization;
using static MiniMax.Models.Enums;

namespace MiniMax.Models.Speech;

public class T2aV2Req
{
    [JsonPropertyName("model")]
    public SpeechModel Model { get; set; } = SpeechModel.Speech28Hd;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;

    [JsonPropertyName("stream_options")]
    public T2AStreamOption? StreamOptions { get; set; }

    [JsonPropertyName("voice_setting")]
    public T2aV2VoiceSetting? VoiceSetting { get; set; }

    [JsonPropertyName("audio_setting")]
    public T2aV2AudioSetting? AudioSetting { get; set; }

    [JsonPropertyName("pronunciation_dict")]
    public T2aV2PronunciationDict? PronunciationDict { get; set; }

    [JsonPropertyName("timbre_weights")]
    public List<TimbreWeight>? TimbreWeights { get; set; }

    [JsonPropertyName("language_boost")]
    public LanguageBoost? LanguageBoost { get; set; }

    [JsonPropertyName("voice_modify")]
    public VoiceModify? VoiceModify { get; set; }

    [JsonPropertyName("subtitle_enable")]
    public bool SubtitleEnable { get; set; } = false;

    [JsonPropertyName("output_format")]
    public T2AOutputFormat OutputFormat { get; set; } = T2AOutputFormat.Hex;

    [JsonPropertyName("aigc_watermark")]
    public bool AigcWatermark { get; set; } = false;
}

public class T2aV2VoiceSetting
{
    [JsonPropertyName("voice_id")]
    public string VoiceId { get; set; } = string.Empty;

    [JsonPropertyName("speed")]
    public float Speed { get; set; } = 1.0f;

    [JsonPropertyName("vol")]
    public float Vol { get; set; } = 1.0f;

    [JsonPropertyName("pitch")]
    public int Pitch { get; set; } = 0;

    [JsonPropertyName("emotion")]
    public SpeechEmotion? Emotion { get; set; }

    [JsonPropertyName("text_normalization")]
    public bool TextNormalization { get; set; } = false;

    [JsonPropertyName("latex_read")]
    public bool LatexRead { get; set; } = false;
}

public class T2aV2AudioSetting
{
    [JsonPropertyName("sample_rate")]
    public AudioSampleRate SampleRate { get; set; } = AudioSampleRate.Rate32000;

    [JsonPropertyName("bitrate")]
    public AudioBitrate Bitrate { get; set; } = AudioBitrate.Rate128000;

    [JsonPropertyName("format")]
    public T2AAudioFormat Format { get; set; } = T2AAudioFormat.Mp3;

    [JsonPropertyName("channel")]
    public int Channel { get; set; } = 1;

    [JsonPropertyName("force_cbr")]
    public bool ForceCbr { get; set; } = false;
}

public class T2aV2PronunciationDict
{
    [JsonPropertyName("tone")]
    public List<string>? Tone { get; set; }
}

public class TimbreWeight
{
    [JsonPropertyName("voice_id")]
    public string VoiceId { get; set; } = string.Empty;

    [JsonPropertyName("weight")]
    public int Weight { get; set; }
}

public class T2AStreamOption
{
    [JsonPropertyName("exclude_aggregated_audio")]
    public bool ExcludeAggregatedAudio { get; set; } = false;
}
