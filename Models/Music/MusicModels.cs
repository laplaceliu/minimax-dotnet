using System.Text.Json.Serialization;
using static MiniMax.Models.Enums;

namespace MiniMax.Models.Music;

public class GenerateMusicReq
{
    [JsonPropertyName("model")]
    public MusicModel Model { get; set; } = MusicModel.Music26;

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("lyrics")]
    public string? Lyrics { get; set; }

    [JsonPropertyName("is_instrumental")]
    public bool IsInstrumental { get; set; } = false;

    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;

    [JsonPropertyName("output_format")]
    public MusicOutputFormat OutputFormat { get; set; } = MusicOutputFormat.Hex;

    [JsonPropertyName("audio_setting")]
    public MusicAudioSetting? AudioSetting { get; set; }

    [JsonPropertyName("aigc_watermark")]
    public bool AigcWatermark { get; set; } = false;

    [JsonPropertyName("lyrics_optimizer")]
    public bool LyricsOptimizer { get; set; } = false;

    [JsonPropertyName("audio_url")]
    public string? AudioUrl { get; set; }

    [JsonPropertyName("audio_base64")]
    public string? AudioBase64 { get; set; }

    [JsonPropertyName("cover_feature_id")]
    public string? CoverFeatureId { get; set; }
}

public class MusicAudioSetting
{
    [JsonPropertyName("sample_rate")]
    public int SampleRate { get; set; } = 44100;

    [JsonPropertyName("bitrate")]
    public int Bitrate { get; set; } = 256000;

    [JsonPropertyName("format")]
    public string Format { get; set; } = "mp3";
}

public class GenerateMusicResp
{
    [JsonPropertyName("data")]
    public MusicData? Data { get; set; }

    [JsonPropertyName("trace_id")]
    public string? TraceId { get; set; }

    [JsonPropertyName("extra_info")]
    public MusicExtraInfo? ExtraInfo { get; set; }

    [JsonPropertyName("analysis_info")]
    public object? AnalysisInfo { get; set; }

    [JsonPropertyName("base_resp")]
    public Core.BaseResp? BaseResp { get; set; }
}

public class MusicData
{
    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("audio")]
    public string? Audio { get; set; }
}

public class MusicExtraInfo
{
    [JsonPropertyName("music_duration")]
    public int MusicDuration { get; set; }

    [JsonPropertyName("music_sample_rate")]
    public int MusicSampleRate { get; set; }

    [JsonPropertyName("music_channel")]
    public int MusicChannel { get; set; }

    [JsonPropertyName("bitrate")]
    public int Bitrate { get; set; }

    [JsonPropertyName("music_size")]
    public int MusicSize { get; set; }
}

public class GenerateLyricsReq
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "lyrics-01";

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("mode")]
    public LyricsMode Mode { get; set; } = LyricsMode.WriteFullSong;

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("lyrics")]
    public string? Lyrics { get; set; }

    [JsonPropertyName("callback_url")]
    public string? CallbackUrl { get; set; }
}

public class GenerateLyricsResp
{
    [JsonPropertyName("task_id")]
    public string TaskId { get; set; } = string.Empty;

    [JsonPropertyName("base_resp")]
    public Core.BaseResp? BaseResp { get; set; }

    [JsonPropertyName("song_title")]
    public string? SongTitle { get; set; }

    [JsonPropertyName("lyrics")]
    public string? Lyrics { get; set; }

    [JsonPropertyName("style_tags")]
    public string? StyleTags { get; set; }
}

public class CoverPreprocessReq
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "cover-preprocess-01";

    [JsonPropertyName("audio_base64")]
    public string? AudioBase64 { get; set; }

    [JsonPropertyName("audio_file_id")]
    public string? AudioFileId { get; set; }

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;
}

public class CoverPreprocessResp
{
    [JsonPropertyName("base_resp")]
    public Core.BaseResp? BaseResp { get; set; }

    [JsonPropertyName("cover_feature_id")]
    public string? CoverFeatureId { get; set; }

    [JsonPropertyName("formatted_lyrics")]
    public string? FormattedLyrics { get; set; }

    [JsonPropertyName("audio_duration")]
    public double? AudioDuration { get; set; }

    [JsonPropertyName("structure_result")]
    public string? StructureResult { get; set; }
}
