using Synergy.Framework.Shared.Abstractions;
using Synergy.Framework.Web.Providers;

namespace Synergy.Framework.Web.Handlers;

public class DefaultUserIdentifierHandler<TUserKey> : IUserIdentifierHandler<TUserKey>
{
    private readonly IUserIdentifierProvider<TUserKey> _userIdentifierProvider;
    public DefaultUserIdentifierHandler(IUserIdentifierProvider<TUserKey> userIdentifierProvider)
    {
        _userIdentifierProvider = userIdentifierProvider;
    }
    public TUserKey UserId => _userIdentifierProvider.UserId;
}
