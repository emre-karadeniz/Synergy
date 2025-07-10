namespace Synergy.Framework.Domain.Base;

/// <summary>
/// Değer nesneleri için temel soyut sınıf.
/// Değer nesneleri, kimlikleri olmayan ancak taşıdıkları değerler aracılığıyla anlam kazanan nesnelerdir.
/// Eşitlikleri, içeriklerinin eşitliği ile belirlenir.
/// </summary>
public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        return GetEqualityComponents().SequenceEqual(((ValueObject)obj).GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    // İsteğe bağlı olarak == ve != operatörleri eklenebilir.
    public static bool operator ==(ValueObject? a, ValueObject? b)
    {
        if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
        {
            return true;
        }

        if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
        {
            return false;
        }

        return a.Equals(b);
    }

    public static bool operator !=(ValueObject? a, ValueObject? b)
    {
        return !(a == b);
    }
}