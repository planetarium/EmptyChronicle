namespace EmptyChronicle;

public class Configuration
{
    public string? AppProtocolVersionToken { get; set; }

    public ushort? Port { get; set; }

    public string? GenesisBlockPath { get; init; }

    public string? StorePath { get; set; }

    public string[]? IceServerStrings { get; set; }

    public string[]? PeerStrings { get; set; }

    public string[]? TrustedAppProtocolVersionSigners { get; set; }
}
