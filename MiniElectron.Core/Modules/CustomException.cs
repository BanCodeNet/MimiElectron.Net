namespace MiniElectron.Core;

public sealed class CustomException : Exception
{
    public int Code { get; init; }
    public new string Message { get; init; }
    public bool Save { get; init; }
    public bool Retry { get; init; }

    /// <summary>
    /// 4xx => 系统错误
    /// 5xx => 业务中断
    /// _ => 未定义的异常
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="retry"></param>
    /// <returns></returns>
    public CustomException(int code, string message = null, bool save = true, bool retry = false) : base()
    {
        Code = code;
        Message = code switch
        {
            >= 400 and < 500 => "系统错误",
            >= 500 and < 600 => "业务中断",
            _ => "未定义的异常类型"
        };
        Message += message != null ? $":{message}" : "";
        Save = save;
        Retry = retry;
    }
}