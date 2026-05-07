using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MiniMax.Core;
using MiniMax.Models;
using MiniMax.Models.Anthropic;
using MiniMax.Models.Chat;
using MiniMax.Models.Files;
using MiniMax.Models.Image;
using MiniMax.Models.Music;
using MiniMax.Models.Speech;
using MiniMax.Models.Video;
using MiniMax.Models.Voice;
using MiniMax.Models.Mcp;
using static MiniMax.Models.Enums;

namespace MiniMax;

    public class MiniMaxClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly JsonSerializerOptions _jsonOptions;

        public MiniMaxClient(string apiKey, HttpClient? httpClient = null)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = { new JsonEnumStringConverter(), new StringToNumberConverter() }
            };
        }

    public McpClient CreateMcpClient(
        string command,
        IEnumerable<string> arguments,
        Dictionary<string, string>? environment = null)
    {
        return new McpClient(command, arguments, environment);
    }

    private void AddAuthHeader(HttpRequestMessage request)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    private async Task<T> SendRequestAsync<T>(
        HttpMethod method,
        string url,
        object? body = null,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(method, url);
        AddAuthHeader(request);

        if (body != null)
        {
            var json = JsonSerializer.Serialize(body, _jsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new MiniMaxException((int)response.StatusCode, content);
        }

        var result = JsonSerializer.Deserialize<T>(content, _jsonOptions);
        if (result == null)
        {
            throw new MiniMaxException(0, "Failed to deserialize response");
        }

        return result;
    }

    private async Task<Stream> SendRequestForStreamAsync(
        HttpMethod method,
        string url,
        object? body = null,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(method, url);
        AddAuthHeader(request);

        if (body != null)
        {
            var json = JsonSerializer.Serialize(body, _jsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var content = await response.Content.ReadAsStreamAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            using var reader = new StreamReader(content);
            var error = await reader.ReadToEndAsync(cancellationToken);
            throw new MiniMaxException((int)response.StatusCode, error);
        }

        return content;
    }

    public async Task<ChatCompletionResp> ChatCompletionAsync(
        ChatCompletionReq request,
        CancellationToken cancellationToken = default)
    {
        var url = "https://api.minimaxi.com/v1/chat/completions";
        return await SendRequestAsync<ChatCompletionResp>(HttpMethod.Post, url, request, cancellationToken);
    }

    public async Task<T2aV2Resp> TextToAudioAsync(
        T2aV2Req request,
        CancellationToken cancellationToken = default)
    {
        var url = "https://api.minimaxi.com/v1/t2a_v2";
        return await SendRequestAsync<T2aV2Resp>(HttpMethod.Post, url, request, cancellationToken);
    }

    public async Task<VideoGenerationResp> GenerateVideoAsync(
        VideoGenerationReq request,
        CancellationToken cancellationToken = default)
    {
        var url = "https://api.minimaxi.com/v1/video_generation";
        return await SendRequestAsync<VideoGenerationResp>(HttpMethod.Post, url, request, cancellationToken);
    }

    public async Task<ImageGenerationResp> GenerateImageAsync(
        ImageGenerationReq request,
        CancellationToken cancellationToken = default)
    {
        var url = "https://api.minimaxi.com/v1/image_generation";
        return await SendRequestAsync<ImageGenerationResp>(HttpMethod.Post, url, request, cancellationToken);
    }

    public async Task<GenerateMusicResp> GenerateMusicAsync(
        GenerateMusicReq request,
        CancellationToken cancellationToken = default)
    {
        var url = "https://api.minimaxi.com/v1/music_generation";
        return await SendRequestAsync<GenerateMusicResp>(HttpMethod.Post, url, request, cancellationToken);
    }

    public async Task<GenerateLyricsResp> GenerateLyricsAsync(
        GenerateLyricsReq request,
        CancellationToken cancellationToken = default)
    {
        var url = "https://api.minimaxi.com/v1/lyrics_generation";
        return await SendRequestAsync<GenerateLyricsResp>(HttpMethod.Post, url, request, cancellationToken);
    }

    public async Task<CoverPreprocessResp> CoverPreprocessAsync(
        CoverPreprocessReq request,
        CancellationToken cancellationToken = default)
    {
        var url = "https://api.minimaxi.com/v1/music_cover_preprocess";
        return await SendRequestAsync<CoverPreprocessResp>(HttpMethod.Post, url, request, cancellationToken);
    }

    public async Task<VoiceCloneResp> CloneVoiceAsync(
        VoiceCloneReq request,
        CancellationToken cancellationToken = default)
    {
        var url = "https://api.minimaxi.com/v1/voice_clone";
        return await SendRequestAsync<VoiceCloneResp>(HttpMethod.Post, url, request, cancellationToken);
    }

    public async Task<VoiceDesignResp> DesignVoiceAsync(
        VoiceDesignReq request,
        CancellationToken cancellationToken = default)
    {
        var url = "https://api.minimaxi.com/v1/voice_design";
        return await SendRequestAsync<VoiceDesignResp>(HttpMethod.Post, url, request, cancellationToken);
    }

    public async Task<GetVoiceResp> GetVoiceAsync(
        Enums.VoiceType voiceType = Enums.VoiceType.All,
        CancellationToken cancellationToken = default)
    {
        var url = "https://api.minimaxi.com/v1/get_voice";
        var request = new GetVoiceReq
        {
            VoiceType = voiceType
        };
        return await SendRequestAsync<GetVoiceResp>(HttpMethod.Post, url, request, cancellationToken);
    }

    public async Task<DeleteVoiceResp> DeleteVoiceAsync(
        DeleteVoiceReq request,
        CancellationToken cancellationToken = default)
    {
        var url = "https://api.minimaxi.com/v1/delete_voice";
        return await SendRequestAsync<DeleteVoiceResp>(HttpMethod.Post, url, request, cancellationToken);
    }

    public async Task<T2AAsyncV2Resp> TextToAudioAsyncCreateAsync(
        T2AAsyncV2Req request,
        CancellationToken cancellationToken = default)
    {
        var url = "https://api.minimaxi.com/v1/t2a_async_v2";
        return await SendRequestAsync<T2AAsyncV2Resp>(HttpMethod.Post, url, request, cancellationToken);
    }

    public async Task<T2AAsyncV2QueryResp> TextToAudioAsyncQueryAsync(
        long taskId,
        CancellationToken cancellationToken = default)
    {
        var url = $"https://api.minimaxi.com/v1/query/t2a_async_query_v2?task_id={taskId}";
        return await SendRequestAsync<T2AAsyncV2QueryResp>(HttpMethod.Get, url, cancellationToken: cancellationToken);
    }

    public async Task<UploadFileResp> UploadFileAsync(
        FilePurpose purpose,
        byte[] fileBytes,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var url = "https://api.minimaxi.com/v1/files/upload";
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        AddAuthHeader(request);

        var boundary = Guid.NewGuid().ToString("N");
        var multipartContent = new MultipartFormDataContent(boundary);

        var purposeString = purpose switch
        {
            FilePurpose.VoiceClone => "voice_clone",
            FilePurpose.PromptAudio => "prompt_audio",
            FilePurpose.T2aAsync => "t2a_async",
            FilePurpose.T2aAsyncInput => "t2a_async_input",
            FilePurpose.VideoGeneration => "video_generation",
            _ => purpose.ToString().ToLowerInvariant()
        };
        var purposeContent = new StringContent(purposeString);
        purposeContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "purpose"
        };
        multipartContent.Add(purposeContent);

        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "file",
            FileName = fileName
        };
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var mediaType = extension switch
        {
            ".mp3" => "audio/mpeg",
            ".m4a" => "audio/mp4",
            ".wav" => "audio/wav",
            _ => "application/octet-stream"
        };
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        multipartContent.Add(fileContent);

        request.Content = multipartContent;

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new MiniMaxException((int)response.StatusCode, responseContent);
        }

        var result = JsonSerializer.Deserialize<UploadFileResp>(responseContent, _jsonOptions);
        if (result == null)
        {
            throw new MiniMaxException(0, "Failed to deserialize response");
        }

        return result;
    }

    public async Task<Stream> RetrieveFileContentAsync(
        long fileId,
        CancellationToken cancellationToken = default)
    {
        var url = $"https://api.minimaxi.com/v1/files/retrieve_content?file_id={fileId}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        AddAuthHeader(request);

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new MiniMaxException((int)response.StatusCode, error);
        }

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    public async Task<RetrieveFileResp> RetrieveFileAsync(
        long fileId,
        CancellationToken cancellationToken = default)
    {
        var url = $"https://api.minimaxi.com/v1/files/retrieve?file_id={fileId}";
        return await SendRequestAsync<RetrieveFileResp>(HttpMethod.Get, url, cancellationToken: cancellationToken);
    }

    public async Task<DeleteFileResp> DeleteFileAsync(
        DeleteFileReq request,
        CancellationToken cancellationToken = default)
    {
        var url = "https://api.minimaxi.com/v1/files/delete";
        return await SendRequestAsync<DeleteFileResp>(HttpMethod.Post, url, request, cancellationToken);
    }

    public async Task<ListFileResp> ListFilesAsync(
        string? purpose = null,
        CancellationToken cancellationToken = default)
    {
        var url = "https://api.minimaxi.com/v1/files/list";
        if (!string.IsNullOrEmpty(purpose))
            url += $"?purpose={purpose}";
        return await SendRequestAsync<ListFileResp>(HttpMethod.Get, url, cancellationToken: cancellationToken);
    }

    public async Task<CreateMessageResp> CreateAnthropicMessageAsync(
        CreateMessageReq request,
        CancellationToken cancellationToken = default)
    {
        var url = "https://api.minimaxi.com/anthropic/v1/messages";
        return await SendRequestAsync<CreateMessageResp>(HttpMethod.Post, url, request, cancellationToken);
    }

    public async Task<QueryVideoGenerationResp> QueryVideoGenerationAsync(
        string taskId,
        CancellationToken cancellationToken = default)
    {
        var url = $"https://api.minimaxi.com/v1/query/video_generation?task_id={taskId}";
        return await SendRequestAsync<QueryVideoGenerationResp>(HttpMethod.Get, url, cancellationToken: cancellationToken);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

public class QueryVideoGenerationResp
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
    public BaseResp? BaseResp { get; set; }
}

public class VideoInfo
{
    [JsonPropertyName("video_url")]
    public string? VideoUrl { get; set; }

    [JsonPropertyName("cover_url")]
    public string? CoverUrl { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }
}

public class McpClient : IDisposable
{
    private readonly Process _process;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;
    private int _requestId;
    private bool _disposed;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public McpClient(string command, IEnumerable<string> arguments, Dictionary<string, string>? environment = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = string.Join(" ", arguments),
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (environment != null)
        {
            foreach (var kvp in environment)
            {
                startInfo.Environment[kvp.Key] = kvp.Value;
            }
        }

        _process = new Process { StartInfo = startInfo };
        _process.Start();

        _reader = _process.StandardOutput;
        _writer = _process.StandardInput;
    }

    public async Task<McpInitializeResult> InitializeAsync(CancellationToken cancellationToken = default)
    {
        var request = new JsonRpcRequest
        {
            Id = ++_requestId,
            Method = "initialize",
            Params = new Dictionary<string, object>
            {
                ["protocolVersion"] = "2024-11-05",
                ["capabilities"] = new Dictionary<string, object>(),
                ["clientInfo"] = new Dictionary<string, object>
                {
                    ["name"] = "minimax-dotnet",
                    ["version"] = "1.0.0"
                }
            }
        };

        var response = await SendRequestAsync<McpInitializeResult>(request, cancellationToken);

        await SendNotificationAsync("notifications/initialized", cancellationToken);

        return response;
    }

    public async Task<List<McpTool>> ListToolsAsync(CancellationToken cancellationToken = default)
    {
        var request = new JsonRpcRequest
        {
            Id = ++_requestId,
            Method = "tools/list"
        };

        var response = await SendRequestAsync<McpToolListResult>(request, cancellationToken);
        return response?.Tools ?? new List<McpTool>();
    }

    public async Task<McpCallToolResult> CallToolAsync(string toolName, Dictionary<string, object>? arguments = null, CancellationToken cancellationToken = default)
    {
        var request = new JsonRpcRequest
        {
            Id = ++_requestId,
            Method = "tools/call",
            Params = new Dictionary<string, object>
            {
                ["name"] = toolName,
                ["arguments"] = arguments ?? new Dictionary<string, object>()
            }
        };

        return await SendRequestAsync<McpCallToolResult>(request, cancellationToken);
    }

    private async Task<T> SendRequestAsync<T>(JsonRpcRequest request, CancellationToken cancellationToken) where T : class
    {
        var json = JsonSerializer.Serialize(request, JsonOptions);
        await _writer.WriteLineAsync(json);
        await _writer.FlushAsync(cancellationToken);

        var responseLine = await _reader.ReadLineAsync(cancellationToken);
        if (string.IsNullOrEmpty(responseLine))
        {
            throw new MiniMaxException(0, "Empty response from MCP server");
        }

        var response = JsonSerializer.Deserialize<JsonRpcResponse>(responseLine, JsonOptions);
        if (response == null)
        {
            throw new MiniMaxException(0, "Failed to deserialize MCP response");
        }

        if (response.Error != null)
        {
            throw new MiniMaxException(response.Error.Code, response.Error.Message);
        }

        if (response.Result == null)
        {
            throw new MiniMaxException(0, "No result in MCP response");
        }

        var resultJson = response.Result.Value.GetRawText();
        return JsonSerializer.Deserialize<T>(resultJson, JsonOptions) ?? throw new MiniMaxException(0, "Failed to deserialize MCP result");
    }

    private async Task SendNotificationAsync(string method, CancellationToken cancellationToken)
    {
        var notification = new { jsonrpc = "2.0", method };
        var json = JsonSerializer.Serialize(notification, JsonOptions);
        await _writer.WriteLineAsync(json);
        await _writer.FlushAsync(cancellationToken);
    }

    public void Dispose()
    {
        if (_disposed) return;

        if (!_process.HasExited)
        {
            _process.Kill(true);
        }

        _reader.Dispose();
        _writer.Dispose();
        _process.Dispose();
        _disposed = true;
    }
}
