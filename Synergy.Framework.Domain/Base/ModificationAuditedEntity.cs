using Synergy.Framework.Domain.Interfaces;

namespace Synergy.Framework.Domain.Base;
/// <summary>
/// Belirtilen tipte Id'ye ve kullanıcı kimliğine sahip, güncellenme denetimli entity'ler için temel soyut sınıf.
/// </summary>
/// <typeparam name="TId">Entity'nin birincil anahtarının tipini belirtir.</typeparam>
/// <typeparam name="TUserKey">Kullanıcı kimliğinin tipini belirtir.</typeparam>
public abstract class ModificationAuditedEntity<TId, TUserKey> : BaseEntity<TId>, IModificationAudited<TUserKey>
{
    /// <inheritdoc />
    public DateTime? UpdatedAt { get; set; }

    /// <inheritdoc />
    public TUserKey? UpdatedByUserId { get; set; }
}

/// <summary>
/// Ortak olarak Guid tipinde kullanıcı kimliği kullanan, belirtilen tipte Id'ye sahip güncellenme denetimli entity'ler için temel soyut sınıf.
/// </summary>
/// <typeparam name="TId">Entity'nin birincil anahtarının tipini belirtir.</typeparam>
public abstract class ModificationAuditedEntity<TId> : ModificationAuditedEntity<TId, Guid>, IModificationAudited
{
}

/// <summary>
/// Ortak olarak int tipinde Id ve Guid tipinde kullanıcı kimliği kullanan güncellenme denetimli entity'ler için temel soyut sınıf.
/// </summary>
public abstract class ModificationAuditedEntity : ModificationAuditedEntity<int, Guid>, IModificationAudited
{
}