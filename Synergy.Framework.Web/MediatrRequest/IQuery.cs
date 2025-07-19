using MediatR;

namespace Synergy.Framework.Web.MediatrRequest;

public interface IQuery<out TResponse> : IRequest<TResponse>;
