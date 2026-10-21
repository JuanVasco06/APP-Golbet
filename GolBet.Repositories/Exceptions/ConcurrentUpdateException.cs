namespace GolBet.Repositories.Exceptions;

public sealed class ConcurrentUpdateException(Exception innerException)
    : Exception("The record changed while the operation was being saved.", innerException);
