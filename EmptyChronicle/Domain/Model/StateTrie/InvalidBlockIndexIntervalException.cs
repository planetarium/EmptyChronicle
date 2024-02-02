namespace EmptyChronicle.Domain.Model.StateTrie;

public class InvalidBlockIndexIntervalException : Exception
{
    public readonly ExceptionReason Reason;

    public InvalidBlockIndexIntervalException(ExceptionReason exceptionReason, string? message = null) : base(message)
    {
        Reason = exceptionReason;
    }

    public enum ExceptionReason
    {
        BaseOverChanged,
        IntervalTooLong,
    }
}