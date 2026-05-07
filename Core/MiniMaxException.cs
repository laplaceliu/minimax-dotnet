using System;

namespace MiniMax.Core;

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