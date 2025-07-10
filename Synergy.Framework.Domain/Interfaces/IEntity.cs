namespace Synergy.Framework.Domain.Interfaces;

/// <summary>
/// Tüm entity'ler için temel arayüzü tanımlar.
/// </summary>
public interface IEntity
{
}

/// <summary>
/// Birincil anahtarı olan entity'ler için temel arayüzü tanımlar.
/// </summary>
/// <typeparam name="TKey">Entity'nin birincil anahtarının tipini belirtir.</typeparam>
public interface IEntity<out TKey> : IEntity
{
    /// <summary>
    /// Entity'nin benzersiz kimliğini temsil eder.
    /// </summary>
    TKey Id { get; }
}
