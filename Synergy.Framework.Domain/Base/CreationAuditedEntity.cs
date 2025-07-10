using Synergy.Framework.Domain.Interfaces;

namespace Synergy.Framework.Domain.Base;
/// <summary>
/// Belirtilen tipte Id'ye ve kullanıcı kimliğine sahip, oluşturulma denetimli entity'ler için temel soyut sınıf.
/// </summary>
/// <typeparam name="TId">Entity'nin birincil anahtarının tipini belirtir.</typeparam>
/// <typeparam name="TUserKey">Kullanıcı kimliğinin tipini belirtir.</typeparam>
public abstract class CreationAuditedEntity<TId, TUserKey> : BaseEntity<TId>, ICreationAudited<TUserKey>
{
    /// <inheritdoc />
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc />
    public required TUserKey CreatedByUserId { get; set; }
}

/// <summary>
/// Ortak olarak Guid tipinde kullanıcı kimliği kullanan, belirtilen tipte Id'ye sahip oluşturulma denetimli entity'ler için temel soyut sınıf.
/// </summary>
/// <typeparam name="TId">Entity'nin birincil anahtarının tipini belirtir.</typeparam>
public abstract class CreationAuditedEntity<TId> : CreationAuditedEntity<TId, Guid>, ICreationAudited
{
}

/// <summary>
/// Ortak olarak int tipinde Id ve Guid tipinde kullanıcı kimliği kullanan oluşturulma denetimli entity'ler için temel soyut sınıf.
/// </summary>
public abstract class CreationAuditedEntity : CreationAuditedEntity<int, Guid>, ICreationAudited
{
}