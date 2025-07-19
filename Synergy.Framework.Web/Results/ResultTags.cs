using Synergy.Framework.Web.Common;

namespace Synergy.Framework.Web.Results;

public readonly struct NotFoundResult
{
    public List<string> Messages { get; }

    public NotFoundResult()
    {
        Messages = [AppMessages.Common.NotFound];
    }

    public NotFoundResult(string message)
    {
        Messages = [message];
    }

    public NotFoundResult(List<string> messages)
    {
        Messages = messages;
    }
}

public readonly struct BadRequestResult
{
    public List<string> Messages { get; }

    public BadRequestResult(string message)
    {
        Messages = [message];
    }

    public BadRequestResult(List<string> messages)
    {
        Messages = messages;
    }
}

public readonly struct SuccessResultData
{
    public List<string> Messages { get; }
    public object Data { get; }

    public SuccessResultData(object data)
    {
        Data = data;
        Messages = [AppMessages.Common.Success];
    }

    public SuccessResultData(object data, string message)
    {
        Data = data;
        Messages = [message];
    }

    public SuccessResultData(object data, List<string> messages)
    {
        Data = data;
        Messages = messages;
    }
}
