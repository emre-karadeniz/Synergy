namespace Synergy.Framework.Domain.Interfaces;

using System;

/// <summary>
/// Oluşturulma denetim bilgisini tutan entity'ler için temel arayüzü tanımlar.
/// </summary>
/// <typeparam name="TUserKey">Kullanıcı kimliğinin tipini belirtir.</typeparam>
public interface ICreationAudited<TUserKey>
{
    /// <summary>
    /// Entity'nin oluşturulma zamanını temsil eder.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Entity'yi oluşturan kullanıcının kimliğini temsil eder.
    /// </summary>
    TUserKey CreatedByUserId { get; set; }
}

/// <summary>
/// Oluşturulma denetim bilgisini tutan entity'ler için temel arayüzü tanımlar (Guid kullanıcı ID'si için).
/// </summary>
public interface ICreationAudited : ICreationAudited<Guid>
{
}