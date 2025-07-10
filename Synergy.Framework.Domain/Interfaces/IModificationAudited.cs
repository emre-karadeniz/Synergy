namespace Synergy.Framework.Domain.Interfaces;

using System;

/// <summary>
/// Güncellenme denetim bilgisini tutan entity'ler için temel arayüzü tanımlar.
/// </summary>
/// <typeparam name="TUserKey">Kullanıcı kimliğinin tipini belirtir.</typeparam>
public interface IModificationAudited<TUserKey>
{
    /// <summary>
    /// Entity'nin güncellenme zamanını temsil eder.
    /// </summary>
    DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Entity'yi güncelleyen kullanıcının kimliğini temsil eder.
    /// </summary>
    TUserKey? UpdatedByUserId { get; set; }
}

/// <summary>
/// Güncellenme denetim bilgisini tutan entity'ler için temel arayüzü tanımlar (Guid kullanıcı ID'si için).
/// </summary>
public interface IModificationAudited : IModificationAudited<Guid>
{
}