using MediatR;

namespace HMS.SharedKernel.Application.Abstractions;

/// <summary>
/// Marker for commands that return a value.
/// Enforces CQRS — all state-changing operations go through ICommand.
/// </summary>
public interface ICommand<TResponse> : IRequest<TResponse> { }

/// <summary>
/// Marker for commands that return nothing.
/// </summary>
public interface ICommand : IRequest { }

/// <summary>
/// Marker for queries (read-only operations).
/// </summary>
public interface IQuery<TResponse> : IRequest<TResponse> { }

/// <summary>
/// Command handler base alias.
/// </summary>
public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>;

public interface ICommandHandler<TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand;

/// <summary>
/// Query handler base alias.
/// </summary>
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>;
