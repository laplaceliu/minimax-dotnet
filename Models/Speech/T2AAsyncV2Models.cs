using System.Text.Json.Serialization;
using static MiniMax.Models.Enums;

namespace MiniMax.Models.Speech;

public class T2AAsyncV2Req
{
    [JsonPropertyName("model")]
    public SpeechModel Model { get; set; } = SpeechModel.Speech28Hd;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("text_file_id")]
    public long? TextFileId { get; set; }

    [JsonPropertyName("voice_setting")]
    public T2AAsyncV2VoiceSetting? VoiceSetting { get; set; }

    [JsonPropertyName("audio_setting")]
    public T2AAsyncV2AudioSetting? AudioSetting { get; set; }

    [JsonPropertyName("pronunciation_dict")]
    public T2AAsyncV2PronunciationDict? PronunciationDict { get; set; }

    [JsonPropertyName("language_boost")]
    public LanguageBoost? LanguageBoost { get; set; }

    [JsonPropertyName("voice_modify")]
    public VoiceModify? VoiceModify { get; set; }

    [JsonPropertyName("aigc_watermark")]
    public bool AigcWatermark { get; set; } = false;
}

public class T2AAsyncV2VoiceSetting
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

    [JsonPropertyName("english_normalization")]
    public bool EnglishNormalization { get; set; } = false;
}

public class T2AAsyncV2AudioSetting
{
    [JsonPropertyName("audio_sample_rate")]
    public AudioSampleRate SampleRate { get; set; } = AudioSampleRate.Rate32000;

    [JsonPropertyName("bitrate")]
    public AudioBitrate Bitrate { get; set; } = AudioBitrate.Rate128000;

    [JsonPropertyName("format")]
    public AudioFormat Format { get; set; } = AudioFormat.Mp3;

    [JsonPropertyName("channel")]
    public int Channel { get; set; } = 1;
}

public class T2AAsyncV2PronunciationDict
{
    [JsonPropertyName("tone")]
    public List<string>? Tone { get; set; }
}

public class VoiceModify
{
    [JsonPropertyName("pitch")]
    public int Pitch { get; set; } = 0;

    [JsonPropertyName("intensity")]
    public int Intensity { get; set; } = 0;

    [JsonPropertyName("timbre")]
    public int Timbre { get; set; } = 0;

    [JsonPropertyName("sound_effects")]
    public VoiceModifySoundEffect? SoundEffects { get; set; }
}

public class T2AAsyncV2Resp
{
    [JsonPropertyName("task_id")]
    public string TaskId { get; set; } = string.Empty;

    [JsonPropertyName("file_id")]
    public long FileId { get; set; }

    [JsonPropertyName("task_token")]
    public string? TaskToken { get; set; }

    [JsonPropertyName("usage_characters")]
    public int UsageCharacters { get; set; }

    [JsonPropertyName("base_resp")]
    public Core.BaseResp? BaseResp { get; set; }
}

public class T2AAsyncV2QueryResp
{
    [JsonPropertyName("task_id")]
    public long TaskId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("file_id")]
    public long? FileId { get; set; }

    [JsonPropertyName("base_resp")]
    public Core.BaseResp? BaseResp { get; set; }
}
