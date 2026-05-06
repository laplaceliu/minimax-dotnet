using System.Collections.Generic;

namespace MiniMax.Core;

public static class ErrorCodes
{
    private static readonly Dictionary<int, ErrorInfo> Errors = new()
    {
        { 1000, new ErrorInfo("未知错误/系统默认错误", "请稍后再试") },
        { 1001, new ErrorInfo("请求超时", "请稍后再试") },
        { 1002, new ErrorInfo("请求频率超限", "请稍后再尝试") },
        { 1004, new ErrorInfo("未授权/Token不匹配/Cookie缺失", "请检查API Key是否填写正确") },
        { 1008, new ErrorInfo("余额不足", "请检查您的账户余额") },
        { 1024, new ErrorInfo("内部错误", "请稍后再试") },
        { 1026, new ErrorInfo("输入内容涉敏", "请调整输入内容") },
        { 1027, new ErrorInfo("输出内容涉敏", "请调整输入内容") },
        { 1033, new ErrorInfo("系统错误/下游服务错误", "请稍后再试") },
        { 1039, new ErrorInfo("Token限制", "请调整max_tokens") },
        { 1041, new ErrorInfo("连接数限制", "请联系我们") },
        { 1042, new ErrorInfo("不可见字符比例超限/非法字符超过10%", "请检查输入内容，是否包含不可见字符或非法字符") },
        { 1043, new ErrorInfo("ASR相似度检查失败", "请检查file_id与text_validation匹配度") },
        { 1044, new ErrorInfo("克隆提示词相似度检查失败", "请检查克隆提示音频和提示词") },
        { 2013, new ErrorInfo("参数错误", "请检查请求参数") },
        { 20132, new ErrorInfo("语音克隆样本或voice_id参数错误", "请检查Voice Cloning接口下的file_id和T2A v2，T2A Large v2接口下的voice_id参数") },
        { 2037, new ErrorInfo("语音时长不符合要求(太长或太短)", "请检查voice_clone file_id文件时长，最少应不低于10秒，最长应不超过5分钟") },
        { 2038, new ErrorInfo("用户语音克隆功能被禁用", "使用语音克隆功能需要完成账户身份认证，请根据您的使用需求在账户管理中中进行个人或企业认证") },
        { 2039, new ErrorInfo("语音克隆voice_id重复", "请修改voice_id，确保未和已有voice_id重复") },
        { 2042, new ErrorInfo("无权访问该voice_id", "请确认是否为该voice_id创建者") },
        { 2045, new ErrorInfo("请求频率增长超限", "请避免请求骤增骤减情况") },
        { 2048, new ErrorInfo("语音克隆提示音频太长", "请调整prompt_audio音频文件时长（<8s）") },
        { 2049, new ErrorInfo("无效的API Key", "请检查API Key") },
        { 2056, new ErrorInfo("超出Token Plan资源限制", "请等待下一个时间段资源释放后，再次尝试") },
    };

    public static ErrorInfo? GetErrorInfo(int statusCode)
    {
        return Errors.TryGetValue(statusCode, out var info) ? info : null;
    }

    public static string GetFriendlyMessage(int statusCode)
    {
        var info = GetErrorInfo(statusCode);
        return info?.FriendlyMessage ?? "请稍后再试或联系我们";
    }
}

public class ErrorInfo
{
    public string Reason { get; }
    public string FriendlyMessage { get; }

    public ErrorInfo(string reason, string friendlyMessage)
    {
        Reason = reason;
        FriendlyMessage = friendlyMessage;
    }

    public override string ToString()
    {
        return $"[{Reason}] {FriendlyMessage}";
    }
}
