using Libplanet.Net;

namespace EmptyChronicle.Hosting;

public class SwarmService : BackgroundService
{
    private readonly Swarm _swarm;
    private readonly BoundPeer[] _peers;

    public SwarmService(Swarm swarm, BoundPeer[] peers)
    {
        _swarm = swarm;
        _peers = peers;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _ = _swarm.WaitForRunningAsync().ContinueWith(_ =>
        {
            var peer = _swarm.AsPeer;
            var result = GetPeerString(peer);
            Console.WriteLine(result);
        }, stoppingToken);

        await _swarm.AddPeersAsync(_peers, default, cancellationToken: stoppingToken).ConfigureAwait(false);
        await _swarm.PreloadAsync(cancellationToken: stoppingToken).ConfigureAwait(false);
        await _swarm.StartAsync(cancellationToken: stoppingToken).ConfigureAwait(false);
    }

    private static string GetPeerString(BoundPeer peer)
    {
        var pubKey = peer.PublicKey.ToString();
        var hostAndPort = peer.ToString().Split('/')[1];
        var host = hostAndPort.Split(':')[0];
        var port = hostAndPort.Split(':')[1];
        return $"peerString: {pubKey},{host},{port}";
    }
}