namespace Synergy.Framework.Web.Providers;

/// <summary>
/// Uygulamada o anki kullanıcının kimliğini (ID) sağlayan servisin genel (generic) arayüzünü tanımlar.
/// </summary>
/// <typeparam name="TUserKey">Kullanıcı kimliğinin tipini belirtir.</typeparam>
public interface IUserIdentifierProvider<TUserKey>
{
    /// <summary>
    /// O anki kullanıcının benzersiz kimliğini (ID) alır. Eğer kullanıcı anonim ise varsayılan bir değer dönebilir.
    /// </summary>
    TUserKey UserId { get; }
}
