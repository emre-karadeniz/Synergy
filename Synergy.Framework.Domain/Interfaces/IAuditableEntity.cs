namespace Synergy.Framework.Domain.Interfaces;

using System;

/// <summary>
/// Denetim (oluşturulma, güncellenme, silinme bilgileri) bilgisini tutan entity'ler için temel arayüzü tanımlar.
/// </summary>
/// <typeparam name="TUserKey">Kullanıcı kimliğinin tipini belirtir.</typeparam>
public interface IAuditableEntity<TUserKey>
{
    /// <summary>
    /// Entity'nin oluşturulma zamanını temsil eder.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Entity'yi oluşturan kullanıcının kimliğini temsil eder.
    /// </summary>
    TUserKey CreatedByUserId { get; set; }

    /// <summary>
    /// Entity'nin güncellenme zamanını temsil eder.
    /// </summary>
    DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Entity'yi güncelleyen kullanıcının kimliğini temsil eder.
    /// </summary>
    TUserKey? UpdatedByUserId { get; set; }

    /// <summary>
    /// Entity'nin silinme zamanını temsil eder.
    /// </summary>
    DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Entity'yi silen kullanıcının kimliğini temsil eder.
    /// </summary>
    TUserKey? DeletedByUserId { get; set; }

    /// <summary>
    /// Entity'nin silinip silinmediğini belirtir (soft delete).
    /// </summary>
    bool Deleted { get; set; }

    /// <summary>
    /// Soft delete işleminin yapılıp yapılmadığını geçici olarak işaretlemek için kullanılır (veritabanına yansımaz).
    /// </summary>
    bool IsSoftDeleting { get; set; }

    /// <summary>
    /// Entity'yi mantıksal olarak siler.
    /// </summary>
    void SoftDelete();
}

/// <summary>
/// Denetim (oluşturulma, güncellenme, silinme bilgileri) bilgisini tutan entity'ler için temel arayüzü tanımlar (Guid kullanıcı ID'si için).
/// </summary>
public interface IAuditableEntity : IAuditableEntity<Guid>
{
}