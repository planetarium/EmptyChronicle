namespace EmptyChronicle.Controller.Dto;

public class BlockDto
{
    public string Hash { get; init; }
    public long Index { get; init; }

    public long TransactionCount => Transactions.Length;
    public DateTimeOffset Timestamp { get; init; }
    public string Miner { get; init; }
    public string StateRootHash { get; init; }
    public TransactionDto[] Transactions { get; init; }
}