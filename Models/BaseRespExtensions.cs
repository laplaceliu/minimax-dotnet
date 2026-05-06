using MiniMax.Models;

namespace MiniMax
{
    public static class BaseRespExtensions
    {
        public static void ThrowIfError(this BaseResp? baseResp)
        {
            if (baseResp?.StatusCode is null or 0)
            {
                return;
            }
            throw new MiniMaxException(baseResp.StatusCode.Value, baseResp.StatusMsg);
        }
    }
}