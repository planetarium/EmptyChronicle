using System.Text.Json.Nodes;

namespace EmptyChronicle.Controller.Dto;

public class TransactionDetailDto
{
    public string Signature { get; init; }

    public string Signer { get; init; }

    public string PublicKey { get; init; }

    public DateTimeOffset UpdateTime { get; init; }

    public DateTimeOffset Timestamp { get; init; }

    public string Status { get; init; }

    public long Nonce { get; init; }

    public long BlockIndex { get; init; }

    public string Id { get; init; }

    public JsonNode[] Actions { get; init; }

    public string[] UpdatedAddresses { get; init; }
}