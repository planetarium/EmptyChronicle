using System.Text.Json.Nodes;

namespace EmptyChronicle.Controller.Dto;

public class TransactionDto
{
    public long BlockIndex { get; init; }
    public string? Signature { get; init; }
    public string[]? UpdatedAddresses { get; init; }
    public string? Id { get; init; }
    public string? PublicKey { get; init; }
    public JsonNode[]? Actions { get; init; }
    public long Nonce { get; init; }
    public string? Signer { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}