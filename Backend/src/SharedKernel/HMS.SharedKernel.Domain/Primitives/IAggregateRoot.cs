namespace HMS.SharedKernel.Primitives;

/// <summary>
/// Marker interface for aggregate roots.
/// Only aggregate roots may be queried/saved directly via the DbContext.
/// </summary>
public interface IAggregateRoot { }
