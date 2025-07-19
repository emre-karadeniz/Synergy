using MediatR;

namespace Synergy.Framework.Web.MediatrRequest;

public interface ICommand<out TResponse> : IRequest<TResponse>;
