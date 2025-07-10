namespace Synergy.Framework.Shared.Abstractions;

public interface IUserIdentifierHandler<TUserKey>
{
    TUserKey UserId { get; }
}
