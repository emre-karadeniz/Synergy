namespace Synergy.Framework.Domain.Base;

using Synergy.Framework.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Belirtilen tipte Id'ye ve kullanıcı kimliğine sahip, tam denetimli (oluşturulma, güncellenme, silinme) entity'ler için temel soyut sınıf.
/// </summary>
/// <typeparam name="TId">Entity'nin birincil anahtarının tipini belirtir.</typeparam>
/// <typeparam name="TUserKey">Kullanıcı kimliğinin tipini belirtir.</typeparam>
public abstract class AuditableEntity<TId, TUserKey> : BaseEntity<TId>, ICreationAudited<TUserKey>, IModificationAudited<TUserKey>, ISoftDelete<TUserKey>
{
    /// <inheritdoc />
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc />
    public required TUserKey CreatedByUserId { get; set; }

    /// <inheritdoc />
    public DateTime? UpdatedAt { get; set; }

    /// <inheritdoc />
    public TUserKey? UpdatedByUserId { get; set; }

    /// <inheritdoc />
    public DateTime? DeletedAt { get; set; }

    /// <inheritdoc />
    public TUserKey? DeletedByUserId { get; set; }

    /// <inheritdoc />
    public bool Deleted { get; set; } = false;

    /// <inheritdoc />
    [NotMapped]
    public bool IsSoftDeleting { get; set; }

    /// <inheritdoc />
    public virtual void SoftDelete()
    {
        IsSoftDeleting = true;
        Deleted = true;
        // DeletedAt ve DeletedByUserId değerleri, veritabanı işlem sırasında atanacaktır.
    }
}

/// <summary>
/// Ortak olarak Guid tipinde kullanıcı kimliği kullanan, belirtilen tipte Id'ye sahip tam denetimli entity'ler için temel soyut sınıf.
/// </summary>
/// <typeparam name="TId">Entity'nin birincil anahtarının tipini belirtir.</typeparam>
public abstract class AuditableEntity<TId> : AuditableEntity<TId, Guid>, IAuditableEntity
{
}

/// <summary>
/// Ortak olarak int tipinde Id ve Guid tipinde kullanıcı kimliği kullanan tam denetimli entity'ler için temel soyut sınıf.
/// </summary>
public abstract class AuditableEntity : AuditableEntity<int, Guid>, IAuditableEntity
{
}