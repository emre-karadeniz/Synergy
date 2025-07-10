namespace Synergy.Framework.Validation.Base;

/// <summary>
/// Domain validasyon hatalarını toplamak için kullanılır.
/// </summary>
public class ValidationNotification
{
    private readonly List<string> _errors = new();

    /// <summary>
    /// Bir hata mesajı ekler.
    /// </summary>
    /// <param name="message">Eklenecek hata mesajı.</param>
    public void AddError(string message) => _errors.Add(message);

    /// <summary>
    /// Bir ValidationResult'tan hata mesajını (varsa) ekler.
    /// </summary>
    /// <param name="validationResult">Eklenecek validasyon sonucu.</param>
    public void AddError(ValidationResult validationResult)
    {
        if (!validationResult.IsValid && !string.IsNullOrEmpty(validationResult.ErrorMessage))
        {
            _errors.Add(validationResult.ErrorMessage);
        }
    }

    /// <summary>
    /// Validasyonun geçerli olup olmadığını belirtir (hata yoksa true).
    /// </summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// Toplanan hata mesajlarının sadece okunabilir listesini döner.
    /// </summary>
    public IReadOnlyList<string> Errors => _errors.AsReadOnly();
}