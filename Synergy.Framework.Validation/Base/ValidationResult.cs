namespace Synergy.Framework.Validation.Base;

/// <summary>
/// Bir validasyon kuralının sonucunu ve hata mesajını temsil eder.
/// </summary>
public record ValidationResult
{
    public bool IsValid { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;

    /// <summary>
    /// Başarılı bir validasyon sonucu oluşturur.
    /// </summary>
    public static ValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Başarısız bir validasyon sonucu oluşturur.
    /// </summary>
    /// <param name="errorMessage">Validasyon başarısız olduğunda döndürülecek hata mesajı.</param>
    public static ValidationResult Fail(string errorMessage) => new() { IsValid = false, ErrorMessage = errorMessage };
}