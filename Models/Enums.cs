using System.Text.Json.Serialization;

namespace MiniMax.Models;

public static class Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ImageModel
    {
        [JsonPropertyName("image-01")]
        Image01,
        [JsonPropertyName("image-01-live")]
        Image01Live
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ImageAspectRatio
    {
        [JsonPropertyName("1:1")]
        R1_1,
        [JsonPropertyName("16:9")]
        R16_9,
        [JsonPropertyName("4:3")]
        R4_3,
        [JsonPropertyName("3:2")]
        R3_2,
        [JsonPropertyName("2:3")]
        R2_3,
        [JsonPropertyName("3:4")]
        R3_4,
        [JsonPropertyName("9:16")]
        R9_16,
        [JsonPropertyName("21:9")]
        R21_9
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ImageResponseFormat
    {
        [JsonPropertyName("url")]
        Url,
        [JsonPropertyName("base64")]
        Base64
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ChatModel
    {
        [JsonPropertyName("MiniMax-M2.7")]
        M2_7,
        [JsonPropertyName("MiniMax-M2.1")]
        M2_1,
        [JsonPropertyName("MiniMax-M2")]
        M2
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum VideoModel
    {
        [JsonPropertyName("MiniMax-Hailuo-2.3")]
        MiniMaxHailuo23,
        [JsonPropertyName("MiniMax-Hailuo-02")]
        MiniMaxHailuo02,
        [JsonPropertyName("T2V-01-Director")]
        T2V01Director,
        [JsonPropertyName("T2V-01")]
        T2V01
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum VideoResolution
    {
        [JsonPropertyName("720P")]
        P720,
        [JsonPropertyName("768P")]
        P768,
        [JsonPropertyName("1080P")]
        P1080
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum VideoProcessStatus
    {
        [JsonPropertyName("Preparing")]
        Preparing,
        [JsonPropertyName("Queueing")]
        Queueing,
        [JsonPropertyName("Processing")]
        Processing,
        [JsonPropertyName("Success")]
        Success,
        [JsonPropertyName("Fail")]
        Fail
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SpeechModel
    {
        [JsonPropertyName("speech-2.8-hd")]
        Speech28Hd,
        [JsonPropertyName("speech-2.8-turbo")]
        Speech28Turbo,
        [JsonPropertyName("speech-2.6-hd")]
        Speech26Hd,
        [JsonPropertyName("speech-2.6-turbo")]
        Speech26Turbo,
        [JsonPropertyName("speech-02-hd")]
        Speech02Hd,
        [JsonPropertyName("speech-02-turbo")]
        Speech02Turbo,
        [JsonPropertyName("speech-01-hd")]
        Speech01Hd,
        [JsonPropertyName("speech-01-turbo")]
        Speech01Turbo
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AudioFormat
    {
        [JsonPropertyName("mp3")]
        Mp3,
        [JsonPropertyName("pcm")]
        Pcm,
        [JsonPropertyName("flac")]
        Flac
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AudioSampleRate
    {
        [JsonPropertyName("8000")]
        Rate8000,
        [JsonPropertyName("16000")]
        Rate16000,
        [JsonPropertyName("22050")]
        Rate22050,
        [JsonPropertyName("24000")]
        Rate24000,
        [JsonPropertyName("32000")]
        Rate32000,
        [JsonPropertyName("44100")]
        Rate44100
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AudioBitrate
    {
        [JsonPropertyName("32000")]
        Rate32000,
        [JsonPropertyName("64000")]
        Rate64000,
        [JsonPropertyName("128000")]
        Rate128000,
        [JsonPropertyName("256000")]
        Rate256000
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SpeechEmotion
    {
        [JsonPropertyName("happy")]
        Happy,
        [JsonPropertyName("sad")]
        Sad,
        [JsonPropertyName("angry")]
        Angry,
        [JsonPropertyName("fearful")]
        Fearful,
        [JsonPropertyName("disgusted")]
        Disgusted,
        [JsonPropertyName("surprised")]
        Surprised,
        [JsonPropertyName("calm")]
        Calm,
        [JsonPropertyName("fluent")]
        Fluent,
        [JsonPropertyName("whisper")]
        Whisper
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum VoiceModifySoundEffect
    {
        [JsonPropertyName("spacious_echo")]
        SpaciousEcho,
        [JsonPropertyName("auditorium_echo")]
        AuditoriumEcho,
        [JsonPropertyName("lofi_telephone")]
        LofiTelephone,
        [JsonPropertyName("robotic")]
        Robotic
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LanguageBoost
    {
        [JsonPropertyName("Chinese")]
        Chinese,
        [JsonPropertyName("Chinese,Yue")]
        ChineseYue,
        [JsonPropertyName("English")]
        English,
        [JsonPropertyName("Arabic")]
        Arabic,
        [JsonPropertyName("Russian")]
        Russian,
        [JsonPropertyName("Spanish")]
        Spanish,
        [JsonPropertyName("French")]
        French,
        [JsonPropertyName("Portuguese")]
        Portuguese,
        [JsonPropertyName("German")]
        German,
        [JsonPropertyName("Turkish")]
        Turkish,
        [JsonPropertyName("Dutch")]
        Dutch,
        [JsonPropertyName("Ukrainian")]
        Ukrainian,
        [JsonPropertyName("Vietnamese")]
        Vietnamese,
        [JsonPropertyName("Indonesian")]
        Indonesian,
        [JsonPropertyName("Japanese")]
        Japanese,
        [JsonPropertyName("Italian")]
        Italian,
        [JsonPropertyName("Korean")]
        Korean,
        [JsonPropertyName("Thai")]
        Thai,
        [JsonPropertyName("Polish")]
        Polish,
        [JsonPropertyName("Romanian")]
        Romanian,
        [JsonPropertyName("Greek")]
        Greek,
        [JsonPropertyName("Czech")]
        Czech,
        [JsonPropertyName("Finnish")]
        Finnish,
        [JsonPropertyName("Hindi")]
        Hindi,
        [JsonPropertyName("Bulgarian")]
        Bulgarian,
        [JsonPropertyName("Danish")]
        Danish,
        [JsonPropertyName("Hebrew")]
        Hebrew,
        [JsonPropertyName("Malay")]
        Malay,
        [JsonPropertyName("Persian")]
        Persian,
        [JsonPropertyName("Slovak")]
        Slovak,
        [JsonPropertyName("Swedish")]
        Swedish,
        [JsonPropertyName("Croatian")]
        Croatian,
        [JsonPropertyName("Filipino")]
        Filipino,
        [JsonPropertyName("Hungarian")]
        Hungarian,
        [JsonPropertyName("Norwegian")]
        Norwegian,
        [JsonPropertyName("Slovenian")]
        Slovenian,
        [JsonPropertyName("Catalan")]
        Catalan,
        [JsonPropertyName("Nynorsk")]
        Nynorsk,
        [JsonPropertyName("Tamil")]
        Tamil,
        [JsonPropertyName("Afrikaans")]
        Afrikaans,
        [JsonPropertyName("auto")]
        Auto
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MusicModel
    {
        [JsonPropertyName("music-2.6")]
        Music26,
        [JsonPropertyName("music-cover")]
        MusicCover,
        [JsonPropertyName("music-2.6-free")]
        Music26Free,
        [JsonPropertyName("music-cover-free")]
        MusicCoverFree
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MusicOutputFormat
    {
        [JsonPropertyName("url")]
        Url,
        [JsonPropertyName("hex")]
        Hex
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum VoiceType
    {
        [JsonPropertyName("system")]
        System,
        [JsonPropertyName("voice_cloning")]
        VoiceCloning,
        [JsonPropertyName("voice_generation")]
        VoiceGeneration,
        [JsonPropertyName("all")]
        All
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FilePurpose
    {
        [JsonPropertyName("voice_clone")]
        VoiceClone,
        [JsonPropertyName("prompt_audio")]
        PromptAudio,
        [JsonPropertyName("t2a_async_input")]
        T2aAsyncInput
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StyleType
    {
        [JsonPropertyName("漫画")]
        Cartoon,
        [JsonPropertyName("元气")]
        Energetic,
        [JsonPropertyName("中世纪")]
        Medieval,
        [JsonPropertyName("水彩")]
        Watercolor
    }
}
