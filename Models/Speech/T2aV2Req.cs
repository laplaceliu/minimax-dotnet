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

    [JsonPropertyName("voice_setting")]
    public T2aV2VoiceSetting? VoiceSetting { get; set; }

    [JsonPropertyName("audio_setting")]
    public T2aV2AudioSetting? AudioSetting { get; set; }

    [JsonPropertyName("language_boost")]
    public LanguageBoost? LanguageBoost { get; set; }
}

public class T2aV2VoiceSetting
{
    [JsonPropertyName("voice_id")]
    public string VoiceId { get; set; } = string.Empty;

    [JsonPropertyName("speed")]
    public float Speed { get; set; } = 1.0f;

    [JsonPropertyName("pitch")]
    public int Pitch { get; set; } = 0;

    [JsonPropertyName("emotion")]
    public SpeechEmotion? Emotion { get; set; }

    [JsonPropertyName("vol")]
    public float Vol { get; set; } = 1.0f;
}

public class T2aV2AudioSetting
{
    [JsonPropertyName("audio_sample_rate")]
    public AudioSampleRate SampleRate { get; set; } = AudioSampleRate.Rate32000;

    [JsonPropertyName("bitrate")]
    public AudioBitrate Bitrate { get; set; } = AudioBitrate.Rate128000;

    [JsonPropertyName("format")]
    public AudioFormat Format { get; set; } = AudioFormat.Mp3;
}
