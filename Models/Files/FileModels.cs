using System.Text.Json.Serialization;
using static MiniMax.Models.Enums;

namespace MiniMax.Models.Files;

public class UploadFileResp
{
    [JsonPropertyName("file")]
    public FileObject? File { get; set; }

    [JsonPropertyName("base_resp")]
    public Core.BaseResp? BaseResp { get; set; }
}

public class RetrieveFileResp
{
    [JsonPropertyName("file")]
    public FileObject? File { get; set; }

    [JsonPropertyName("base_resp")]
    public Core.BaseResp? BaseResp { get; set; }
}

public class DeleteFileReq
{
    [JsonPropertyName("file_id")]
    public long FileId { get; set; }

    [JsonPropertyName("purpose")]
    public string Purpose { get; set; } = string.Empty;
}

public class DeleteFileResp
{
    [JsonPropertyName("file_id")]
    public long FileId { get; set; }

    [JsonPropertyName("base_resp")]
    public Core.BaseResp? BaseResp { get; set; }
}

public class ListFileResp
{
    [JsonPropertyName("files")]
    public List<FileObject>? Files { get; set; }

    [JsonPropertyName("base_resp")]
    public Core.BaseResp? BaseResp { get; set; }
}

public class FileObject
{
    [JsonPropertyName("file_id")]
    public long FileId { get; set; }

    [JsonPropertyName("bytes")]
    public long Bytes { get; set; }

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("filename")]
    public string Filename { get; set; } = string.Empty;

    [JsonPropertyName("purpose")]
    public string Purpose { get; set; } = string.Empty;

    [JsonPropertyName("download_url")]
    public string? DownloadUrl { get; set; }
}
