using MediatR;

namespace Synergy.Framework.Web.MediatrRequest;

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
where TCommand : ICommand<TResponse>;

