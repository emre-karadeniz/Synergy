using Synergy.Framework.Domain.Interfaces;

namespace Synergy.Framework.Domain.Base;

/// <summary>
/// Eğer tablonun ID alanı yoksa bu base class kullanılabilir.
/// </summary>
public abstract class BaseEntity : IEntity
{
}

/// <summary>
/// Ortak olarak belirtilen tipte Id kullanan entity'ler için temel soyut sınıf.
/// </summary>
/// <typeparam name="TId">Entity'nin birincil anahtarının tipini belirtir.</typeparam>
public abstract class BaseEntity<TId> : BaseEntity, IEntity<TId>
{
    /// <summary>
    /// Entity'nin benzersiz kimliğini temsil eder.
    /// </summary>
    public TId Id { get; protected set; } = GenerateDefaultId();

    private static TId GenerateDefaultId()
    {
        return typeof(TId) == typeof(Guid)
            ? (TId)(object)Guid.NewGuid()
            : default!;
    }
}

/// <summary>
/// ID alanı olmayan ve birden fazla birincil anahtara sahip olabilen entity'ler için temel soyut sınıf.
/// </summary>
public abstract class CompositeKeyEntity : BaseEntity
{
    // ID alanı yoktur, birden fazla PK olabilir.
}
