namespace MiniMax.Core;

public static class BaseRespExtensions
{
    public static void ThrowIfError(this BaseResp? baseResp)
    {
        if (baseResp == null || baseResp.StatusCode == 0)
        {
            return;
        }
        throw new MiniMaxException(baseResp.StatusCode, baseResp.StatusMsg);
    }
}
