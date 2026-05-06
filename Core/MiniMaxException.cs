using System;
using System.Runtime.Serialization;

namespace MiniMax.Core;

[Serializable]
public class MiniMaxException : Exception
{
    public int StatusCode { get; }
    public string? Reason { get; }

    public MiniMaxException(int statusCode, string? statusMsg = null) : base(FormatMessage(statusCode, statusMsg))
    {
        StatusCode = statusCode;
        var errorInfo = ErrorCodes.GetErrorInfo(statusCode);
        Reason = errorInfo?.Reason ?? statusMsg;
    }

    public MiniMaxException(int statusCode, string? statusMsg, Exception innerException)
        : base(FormatMessage(statusCode, statusMsg), innerException)
    {
        StatusCode = statusCode;
        var errorInfo = ErrorCodes.GetErrorInfo(statusCode);
        Reason = errorInfo?.Reason ?? statusMsg;
    }

    protected MiniMaxException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        StatusCode = info.GetInt32(nameof(StatusCode));
        Reason = info.GetString(nameof(Reason));
    }

    [Obsolete("This API supports obsolete formatter-based serialization.")]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(StatusCode), StatusCode);
        info.AddValue(nameof(Reason), Reason);
    }

    private static string FormatMessage(int statusCode, string? statusMsg)
    {
        var errorInfo = ErrorCodes.GetErrorInfo(statusCode);
        if (errorInfo != null)
        {
            return $"[{statusCode}] {errorInfo.Reason} - {errorInfo.FriendlyMessage}" +
                   (string.IsNullOrEmpty(statusMsg) ? "" : $" (服务器返回: {statusMsg})");
        }
        return string.IsNullOrEmpty(statusMsg)
            ? $"[{statusCode}] 请稍后再试或联系我们"
            : $"[{statusCode}] {statusMsg}";
    }
}
