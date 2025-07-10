using Synergy.Framework.Web.Common;
using System.Net;

namespace Synergy.Framework.Web.Results;

public class Result
{
    public IReadOnlyList<string> Messages { get; } = [];
    public int StatusCode { get; }
    public bool IsSuccess => StatusCode is >= 200 and < 300;

    protected Result(List<string> messages, HttpStatusCode statusCode)
    {
        Messages = messages;
        StatusCode = (int)statusCode;
    }

    //Başarılı işlem
    public static Result Success(string message = "")
        => new([string.IsNullOrEmpty(message) ? AppMessages.Common.Success : message], HttpStatusCode.OK);

    //Kaydetme işlemi ancak data dönmeyen
    public static Result Created(string message = "") =>
        new([string.IsNullOrEmpty(message) ? AppMessages.Common.Add : message], HttpStatusCode.Created);

    //Güncelleme işlemi
    public static Result Updated(string message = "") =>
        new([string.IsNullOrEmpty(message) ? AppMessages.Common.Update : message], HttpStatusCode.OK);

    //Silme işlemi
    public static Result Deleted(string message = "") =>
        new([string.IsNullOrEmpty(message) ? AppMessages.Common.Delete : message], HttpStatusCode.OK);

    // Kullanıcı hataları
    public static Result BadRequest(string message) =>
        new([message], HttpStatusCode.BadRequest);

    //Validasyon yada çoklu hatalar
    public static Result BadRequest(List<string> messages) =>
        new(messages, HttpStatusCode.BadRequest);

    //Kayıt bulunamadı
    public static Result NotFound(string message = "") =>
        new([string.IsNullOrEmpty(message) ? AppMessages.Common.NotFound : message], HttpStatusCode.NotFound);

    // Geçersiz login
    public static Result Unauthorized(string message = "") =>
        new([string.IsNullOrEmpty(message) ? AppMessages.Login.ValidToken : message], HttpStatusCode.Unauthorized);

    // Yetkisiz erişim
    public static Result Forbidden(string message = "") =>
        new([string.IsNullOrEmpty(message) ? AppMessages.Login.AccessDenied : message], HttpStatusCode.Forbidden);

    // RateLimiting
    public static Result RateLimiting(string message = "") =>
        new([string.IsNullOrEmpty(message) ? AppMessages.Common.RateLimiting : message], HttpStatusCode.Forbidden);

    // Genel hata (500 vb.)
    public static Result Failure(string message = "", HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError)
        => new([string.IsNullOrEmpty(message) ? AppMessages.Common.Error : message], httpStatusCode);
}

public class Result<T> : Result
{
    public T? Data { get; }

    private Result(T? data, List<string> messages, HttpStatusCode statusCode) : base(messages, statusCode)
    {
        Data = data;
    }

    // 🔥 **Implicit Operator ile T türünü otomatik Result<T> olarak dönüştürme**
    public static implicit operator Result<T>(T? data) => new(data, [AppMessages.Common.Success], HttpStatusCode.OK);
    public static implicit operator Result<T>(SuccessResultData tag) => new((T)tag.Data!, tag.Messages, HttpStatusCode.OK);


    public static implicit operator Result<T>(NotFoundResult tag) => new(default, tag.Messages, HttpStatusCode.NotFound);
    public static implicit operator Result<T>(BadRequestResult tag) => new(default, tag.Messages, HttpStatusCode.BadRequest);
}
