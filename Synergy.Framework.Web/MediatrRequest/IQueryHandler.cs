using MediatR;

namespace Synergy.Framework.Web.MediatrRequest;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
 where TQuery : IQuery<TResponse>;

