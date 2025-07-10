using System.Text.RegularExpressions;

namespace Synergy.Framework.Validation.Base;

/// <summary>
/// Çeşitli veri tipleri için standart validasyon kurallarını ve mesajlarını içerir.
/// Her metot bir ValidationResult döner.
/// </summary>
public static class ValidationRules
{
    // Regex Desenleri
    private const string TcKimlikNoRegex = @"^\d{11}$";
    private const string PhoneNumberRegex = @"^\d{10,15}$";
    private const string EmailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    /// <summary>
    /// Belirtilen T.C. Kimlik Numarasının geçerli olup olmadığını kontrol eder.
    /// </summary>
    /// <param name="tcNo">Kontrol edilecek T.C. Kimlik Numarası.</param>
    /// <param name="overrideMessage">Varsayılan mesaj yerine kullanılacak özel mesaj.</param>
    /// <returns>ValidationResult nesnesi.</returns>
    public static ValidationResult IsValidTCNo(string tcNo, string? overrideMessage = null)
    {
        if (string.IsNullOrWhiteSpace(tcNo) || !Regex.IsMatch(tcNo, TcKimlikNoRegex))
            return ValidationResult.Fail(overrideMessage ?? "Please enter a valid T.C. Identity No.");
        return ValidationResult.Success();
    }

    /// <summary>
    /// Belirtilen telefon numarasının geçerli olup olmadığını kontrol eder.
    /// </summary>
    /// <param name="phone">Kontrol edilecek telefon numarası.</param>
    /// <param name="overrideMessage">Varsayılan mesaj yerine kullanılacak özel mesaj.</param>
    /// <returns>ValidationResult nesnesi.</returns>
    public static ValidationResult IsValidPhoneNumber(string phone, string? overrideMessage = null)
    {
        if (string.IsNullOrWhiteSpace(phone) || !Regex.IsMatch(phone, PhoneNumberRegex))
            return ValidationResult.Fail(overrideMessage ?? "Please enter a valid phone number.");
        return ValidationResult.Success();
    }

    /// <summary>
    /// Belirtilen e-posta adresinin geçerli olup olmadığını kontrol eder.
    /// </summary>
    /// <param name="email">Kontrol edilecek e-posta adresi.</param>
    /// <param name="overrideMessage">Varsayılan mesaj yerine kullanılacak özel mesaj.</param>
    /// <returns>ValidationResult nesnesi.</returns>
    public static ValidationResult IsValidEmail(string email, string? overrideMessage = null)
    {
        if (string.IsNullOrWhiteSpace(email) || !Regex.IsMatch(email, EmailRegex))
            return ValidationResult.Fail(overrideMessage ?? "Please enter a valid email address.");
        return ValidationResult.Success();
    }

    /// <summary>
    /// Bir stringin sadece rakamlardan oluşup oluşmadığını kontrol eder.
    /// </summary>
    /// <param name="input">Kontrol edilecek string.</param>
    /// <param name="overrideMessage">Varsayılan mesaj yerine kullanılacak özel mesaj.</param>
    /// <returns>ValidationResult nesnesi.</returns>
    public static ValidationResult IsOnlyDigits(string input, string? overrideMessage = null)
    {
        if (string.IsNullOrWhiteSpace(input) || !input.All(char.IsDigit))
            return ValidationResult.Fail(overrideMessage ?? "The input must contain only digits.");
        return ValidationResult.Success();
    }

    /// <summary>
    /// Bir stringin sadece harflerden oluşup oluşmadığını kontrol eder.
    /// </summary>
    /// <param name="input">Kontrol edilecek string.</param>
    /// <param name="overrideMessage">Varsayılan mesaj yerine kullanılacak özel mesaj.</param>
    /// <returns>ValidationResult nesnesi.</returns>
    public static ValidationResult IsOnlyLetters(string input, string? overrideMessage = null)
    {
        if (string.IsNullOrWhiteSpace(input) || !input.All(char.IsLetter))
            return ValidationResult.Fail(overrideMessage ?? "The input must contain only letters.");
        return ValidationResult.Success();
    }

    /// <summary>
    /// Bir stringin geçerli ve pozitif bir miktar (decimal) olup olmadığını kontrol eder.
    /// </summary>
    /// <param name="amount">Kontrol edilecek string miktar.</param>
    /// <param name="overrideMessage">Varsayılan mesaj yerine kullanılacak özel mesaj.</param>
    /// <returns>ValidationResult nesnesi.</returns>
    public static ValidationResult IsValidAmount(string amount, string? overrideMessage = null)
    {
        if (!decimal.TryParse(amount, out var value) || value < 0)
            return ValidationResult.Fail(overrideMessage ?? "Amount format is invalid or less than zero.");
        return ValidationResult.Success();
    }

    /// <summary>
    /// Belirtilen tarihin minimum tarihten sonra veya eşit olup olmadığını kontrol eder.
    /// </summary>
    /// <param name="date">Kontrol edilecek tarih.</param>
    /// <param name="minDate">Minimum tarih. Varsayılan: 1900-01-01.</param>
    /// <param name="overrideMessage">Varsayılan mesaj yerine kullanılacak özel mesaj.</param>
    /// <returns>ValidationResult nesnesi.</returns>
    public static ValidationResult IsAfterMinDate(DateTime date, DateTime? minDate = null, string? overrideMessage = null)
    {
        minDate ??= new DateTime(1900, 1, 1);
        if (date < minDate.Value)
            return ValidationResult.Fail(overrideMessage ?? $"Date must be after {minDate.Value:yyyy-MM-dd}.");
        return ValidationResult.Success();
    }

    /// <summary>
    /// Zorunlu olan string alan için kontrol. Minimum ve opsiyonel olarak maksimum uzunluk alabilir.
    /// </summary>
    /// <param name="value">Kontrol edilecek string değer.</param>
    /// <param name="minLength">Minimum uzunluk.</param>
    /// <param name="maxLength">Opsiyonel maksimum uzunluk.</param>
    /// <param name="propertyName">Mesajda kullanılacak alan adı.</param>
    /// <param name="overrideMessage">Varsayılan mesaj yerine kullanılacak özel mesaj.</param>
    /// <returns>ValidationResult nesnesi.</returns>
    public static ValidationResult IsRequiredStringInRange(string? value, int minLength, int? maxLength = null, string? propertyName = null, string? overrideMessage = null)
    {
        propertyName ??= "Value"; // Varsayılan değer

        if (string.IsNullOrWhiteSpace(value))
            return ValidationResult.Fail(overrideMessage ?? $"{propertyName} is required.");

        if (value.Length < minLength)
            return ValidationResult.Fail(overrideMessage ?? $"{propertyName} must be at least {minLength} characters long.");

        if (maxLength.HasValue && value.Length > maxLength.Value)
            return ValidationResult.Fail(overrideMessage ?? $"{propertyName} must be a maximum of {maxLength.Value} characters long.");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Opsiyonel string alan için kontrol. Boş olabilir ama doluysa maksimum uzunluğu geçemez.
    /// </summary>
    /// <param name="value">Kontrol edilecek string değer.</param>
    /// <param name="maxLength">Opsiyonel maksimum uzunluk.</param>
    /// <param name="propertyName">Mesajda kullanılacak alan adı.</param>
    /// <param name="overrideMessage">Varsayılan mesaj yerine kullanılacak özel mesaj.</param>
    /// <returns>ValidationResult nesnesi.</returns>
    public static ValidationResult IsOptionalStringWithMaxLength(string? value, int? maxLength = null, string? propertyName = null, string? overrideMessage = null)
    {
        propertyName ??= "Value";

        if (string.IsNullOrWhiteSpace(value))
            return ValidationResult.Success(); // Opsiyonel olduğu için boş ise geçerli

        if (maxLength.HasValue && value.Length > maxLength.Value)
            return ValidationResult.Fail(overrideMessage ?? $"{propertyName} must be a maximum of {maxLength.Value} characters long.");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Bir int alanın 0 veya daha büyük olup olmadığını kontrol eder.
    /// </summary>
    /// <param name="value">Kontrol edilecek int değer.</param>
    /// <param name="propertyName">Mesajda kullanılacak alan adı.</param>
    /// <param name="overrideMessage">Varsayılan mesaj yerine kullanılacak özel mesaj.</param>
    /// <returns>ValidationResult nesnesi.</returns>
    public static ValidationResult IsZeroOrPositive(int value, string? propertyName = null, string? overrideMessage = null)
    {
        propertyName ??= "Value";
        if (value < 0)
            return ValidationResult.Fail(overrideMessage ?? $"{propertyName} must be greater than or equal to zero.");
        return ValidationResult.Success();
    }

    /// <summary>
    /// Bir decimal alanın 0 veya daha büyük olup olmadığını kontrol eder.
    /// </summary>
    /// <param name="value">Kontrol edilecek decimal değer.</param>
    /// <param name="propertyName">Mesajda kullanılacak alan adı.</param>
    /// <param name="overrideMessage">Varsayılan mesaj yerine kullanılacak özel mesaj.</param>
    /// <returns>ValidationResult nesnesi.</returns>
    public static ValidationResult IsZeroOrPositive(decimal value, string? propertyName = null, string? overrideMessage = null)
    {
        propertyName ??= "Value";
        if (value < 0)
            return ValidationResult.Fail(overrideMessage ?? $"{propertyName} must be greater than or equal to zero.");
        return ValidationResult.Success();
    }

    /// <summary>
    /// Bir double alanın 0 veya daha büyük olup olmadığını kontrol eder.
    /// </summary>
    /// <param name="value">Kontrol edilecek double değer.</param>
    /// <param name="propertyName">Mesajda kullanılacak alan adı.</param>
    /// <param name="overrideMessage">Varsayılan mesaj yerine kullanılacak özel mesaj.</param>
    /// <returns>ValidationResult nesnesi.</returns>
    public static ValidationResult IsZeroOrPositive(double value, string? propertyName = null, string? overrideMessage = null)
    {
        propertyName ??= "Value";
        if (value < 0)
            return ValidationResult.Fail(overrideMessage ?? $"{propertyName} must be greater than or equal to zero.");
        return ValidationResult.Success();
    }

    /// <summary>
    /// Bir float alanın 0 veya daha büyük olup olmadığını kontrol eder.
    /// </summary>
    /// <param name="value">Kontrol edilecek float değer.</param>
    /// <param name="propertyName">Mesajda kullanılacak alan adı.</param>
    /// <param name="overrideMessage">Varsayılan mesaj yerine kullanılacak özel mesaj.</param>
    /// <returns>ValidationResult nesnesi.</returns>
    public static ValidationResult IsZeroOrPositive(float value, string? propertyName = null, string? overrideMessage = null)
    {
        propertyName ??= "Value";
        if (value < 0)
            return ValidationResult.Fail(overrideMessage ?? $"{propertyName} must be greater than or equal to zero.");
        return ValidationResult.Success();
    }

    /// <summary>
    /// Bir int alanın 0'dan büyük olup olmadığını kontrol eder.
    /// </summary>
    /// <param name="value">Kontrol edilecek int değer.</param>
    /// <param name="propertyName">Mesajda kullanılacak alan adı.</param>
    /// <param name="overrideMessage">Varsayılan mesaj yerine kullanılacak özel mesaj.</param>
    /// <returns>ValidationResult nesnesi.</returns>
    public static ValidationResult IsGreaterThanZero(int value, string? propertyName = null, string? overrideMessage = null)
    {
        propertyName ??= "Value";
        if (value <= 0)
            return ValidationResult.Fail(overrideMessage ?? $"{propertyName} must be greater than zero.");
        return ValidationResult.Success();
    }

    /// <summary>
    /// Bir decimal alanın 0'dan büyük olup olmadığını kontrol eder.
    /// </summary>
    /// <param name="value">Kontrol edilecek decimal değer.</param>
    /// <param name="propertyName">Mesajda kullanılacak alan adı.</param>
    /// <param name="overrideMessage">Varsayılan mesaj yerine kullanılacak özel mesaj.</param>
    /// <returns>ValidationResult nesnesi.</returns>
    public static ValidationResult IsGreaterThanZero(decimal value, string? propertyName = null, string? overrideMessage = null)
    {
        propertyName ??= "Value";
        if (value <= 0)
            return ValidationResult.Fail(overrideMessage ?? $"{propertyName} must be greater than zero.");
        return ValidationResult.Success();
    }

    /// <summary>
    /// Bir double alanın 0'dan büyük olup olmadığını kontrol eder.
    /// </summary>
    /// <param name="value">Kontrol edilecek double değer.</param>
    /// <param name="propertyName">Mesajda kullanılacak alan adı.</param>
    /// <param name="overrideMessage">Varsayılan mesaj yerine kullanılacak özel mesaj.</param>
    /// <returns>ValidationResult nesnesi.</returns>
    public static ValidationResult IsGreaterThanZero(double value, string? propertyName = null, string? overrideMessage = null)
    {
        propertyName ??= "Value";
        if (value <= 0)
            return ValidationResult.Fail(overrideMessage ?? $"{propertyName} must be greater than zero.");
        return ValidationResult.Success();
    }

    /// <summary>
    /// Bir float alanın 0'dan büyük olup olmadığını kontrol eder.
    /// </summary>
    /// <param name="value">Kontrol edilecek float değer.</param>
    /// <param name="propertyName">Mesajda kullanılacak alan adı.</param>
    /// <param name="overrideMessage">Varsayılan mesaj yerine kullanılacak özel mesaj.</param>
    /// <returns>ValidationResult nesnesi.</returns>
    public static ValidationResult IsGreaterThanZero(float value, string? propertyName = null, string? overrideMessage = null)
    {
        propertyName ??= "Value";
        if (value <= 0)
            return ValidationResult.Fail(overrideMessage ?? $"{propertyName} must be greater than zero.");
        return ValidationResult.Success();
    }

    /// <summary>
    /// Başlangıç tarihinin bitiş tarihinden büyük olmamasını kontrol eder.
    /// </summary>
    /// <param name="start">Başlangıç tarihi.</param>
    /// <param name="end">Bitiş tarihi.</param>
    /// <param name="overrideMessage">Varsayılan mesaj yerine kullanılacak özel mesaj.</param>
    /// <returns>ValidationResult nesnesi.</returns>
    public static ValidationResult IsStartDateBeforeOrEqualEndDate(DateTime? start, DateTime? end, string? overrideMessage = null)
    {
        if (!start.HasValue || !end.HasValue)
            return ValidationResult.Success(); // Biri boşsa kontrol etme

        if (start.Value > end.Value)
            return ValidationResult.Fail(overrideMessage ?? "The start date cannot be greater than the end date.");

        return ValidationResult.Success();
    }
}