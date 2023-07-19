using System.Text.Json.Nodes;
using Libplanet.Tx;

namespace EmptyChronicle.Controller.Dto;

public class TransactionDto
{
    public long BlockIndex { get; set; }
    public string Signature { get; set; }
    public string[] UpdatedAddresses { get; set; }
    public string Id { get; set; }
    public string PublicKey { get; set; }
    public JsonNode[] Actions { get; set; }
    public long Nonce { get; set; }
    public string Signer { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}