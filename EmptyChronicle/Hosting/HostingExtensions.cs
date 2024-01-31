using System.Collections.Immutable;
using System.IO.Compression;
using Bencodex;
using Libplanet.Store;
using Libplanet.Action;
using Libplanet.Action.Loader;
using Libplanet.Blockchain;
using Libplanet.Blockchain.Policies;
using Libplanet.Types.Blocks;
using Libplanet.Crypto;
using Libplanet.Net;
using Libplanet.Net.Options;
using Libplanet.Net.Transports;
using Libplanet.Common;
using Libplanet.Action.State;
using Libplanet.Extensions.ForkableActionEvaluator;
using Libplanet.Extensions.PluggedActionEvaluator;
using Libplanet.RocksDBStore;
using Libplanet.Store.Trie;
using Nekoyume.Action.Loader;
using Nekoyume.Blockchain.Policy;
using Serilog;
using HostOptions = Libplanet.Net.Options.HostOptions;

namespace EmptyChronicle.Hosting;

public static class HostingExtensions
{
    public static IServiceCollection AddLibplanetServices(
        this IServiceCollection services,
        Configuration configuration)
    {
        var peers = configuration.PeerStrings?.Select(BoundPeer.ParsePeer).ToArray()
                    ?? Array.Empty<BoundPeer>();

        // Options
        services.AddSingleton<HostOptions>(_ => new HostOptions(
            null,
            configuration.IceServerStrings?.Select(serverString => new IceServer(serverString))
            ?? Array.Empty<IceServer>(),
            configuration.Port ?? 31234
        ));

        // BlockChain
        services
            .AddSingleton<IBlockPolicy>(_ => new BlockPolicySource().GetPolicy())
            .AddSingleton<IStagePolicy>(_ => new VolatileStagePolicy())
            .AddSingleton<IStore>(_ => new RocksDBStore(configuration.StorePath))
            .AddSingleton<IKeyValueStore>(_ => new RocksDBKeyValueStore(Path.Combine(configuration.StorePath ?? "planet-node-chain", "states")))
            .AddSingleton<IStateStore>(provider => new TrieStateStore(provider.GetRequiredService<IKeyValueStore>()))
            .AddSingleton<Block>(provider =>
            {
                if (configuration.GenesisBlockPath is { } path)
                {
                    using var client = new HttpClient();
                    var codec = new Codec();

                    var uri = new Uri(path);
                    var block = BlockMarshaler.UnmarshalBlock(
                        (Bencodex.Types.Dictionary)codec.Decode(client.GetByteArrayAsync(uri).Result)
                    );

                    return block;
                }

                var store = provider.GetRequiredService<IStore>();

                if (store.GetCanonicalChainId() is not { } cid ||
                    store.CountIndex(cid) <= 0)
                {
                    throw new Exception("Invalid CanonicalChainId");
                }

                var genesisHash = store.IterateIndexes(cid, 0, 1).Single();
                return store.GetBlock(genesisHash);
            })
            .AddSingleton<IBlockChainStates, BlockChainStates>()
            .AddSingleton<IActionLoader>(_ => new NCActionLoader())
            .AddSingleton<IActionEvaluator>(provider =>
            {
                var typeName = "Lib9c.Plugin.PluginActionEvaluator";
                var keyValueStore = provider.GetRequiredService<IKeyValueStore>();
                var actionLoader = provider.GetRequiredService<IActionLoader>();
                if (configuration.ActionEvaluatorRanges is not { } ranges || ranges.Length == 0)
                {
                    throw new ArgumentException();
                }
                var pairs = ranges.Select(range => (
                    (range.StartBlockIndex, range.EndBlockIndex),
                    (IActionEvaluator)new PluggedActionEvaluator(
                        ResolvePluginPath(range.PluginPath),
                        typeName,
                        keyValueStore,
                        actionLoader
                    )));
                return new ForkableActionEvaluator(pairs, actionLoader);
            })
            .AddSingleton<BlockChain>(provider =>
            {
                var store = provider.GetRequiredService<IStore>();
                if (store.GetCanonicalChainId() is { } canonId)
                {
                    return new BlockChain(
                        provider.GetRequiredService<IBlockPolicy>(),
                        provider.GetRequiredService<IStagePolicy>(),
                        provider.GetRequiredService<IStore>(),
                        provider.GetRequiredService<IStateStore>(),
                        provider.GetRequiredService<Block>(),
                        provider.GetRequiredService<IBlockChainStates>(),
                        provider.GetRequiredService<IActionEvaluator>());
                }

                return BlockChain.Create(
                    provider.GetRequiredService<IBlockPolicy>(),
                    provider.GetRequiredService<IStagePolicy>(),
                    provider.GetRequiredService<IStore>(),
                    provider.GetRequiredService<IStateStore>(),
                    provider.GetRequiredService<Block>(),
                    provider.GetRequiredService<IActionEvaluator>());
            });

        // Transport
        services
            .AddSingleton<AppProtocolVersionOptions>(_ => new AppProtocolVersionOptions
            {
                AppProtocolVersion = AppProtocolVersion.FromToken(configuration.AppProtocolVersionToken),
                TrustedAppProtocolVersionSigners = configuration.TrustedAppProtocolVersionSigners
                                                       ?.Select(x => new PublicKey(ByteUtil.ParseHex(x)))
                                                       .ToImmutableHashSet()
                                                   ?? ImmutableHashSet<PublicKey>.Empty,
            })
            .AddSingleton<ITransport>(provider => NetMQTransport.Create(
                new PrivateKey(),
                provider.GetRequiredService<AppProtocolVersionOptions>(),
                provider.GetRequiredService<HostOptions>()
            ).ConfigureAwait(false).GetAwaiter().GetResult());

        // Swarm and SwarmService
        services
            .AddSingleton(provider => new Swarm(
                provider.GetRequiredService<BlockChain>(),
                new PrivateKey(),
                provider.GetRequiredService<ITransport>()
            ))
            .AddHostedService<SwarmService>(provider => new SwarmService(
                provider.GetRequiredService<Swarm>(),
                peers
            ));

        return services;
    }

    private static string ResolvePluginPath(string path) =>
        Uri.IsWellFormedUriString(path, UriKind.Absolute)
            ? DownloadPlugin(path).Result
            : path;

    private static async Task<string> DownloadPlugin(string url)
    {
        var path = Path.Combine(Environment.CurrentDirectory, "plugins");
        Directory.CreateDirectory(path);
        var hashed = url.GetHashCode().ToString();
        var logger = Log.ForContext("LibplanetNodeService", hashed);
        using var httpClient = new HttpClient();
        var downloadPath = Path.Join(path, hashed + ".zip");
        var extractPath = Path.Join(path, hashed);
        logger.Debug("Downloading...");
        await File.WriteAllBytesAsync(downloadPath, await httpClient.GetByteArrayAsync(url));
        logger.Debug("Finished downloading.");
        logger.Debug("Extracting...");
        ZipFile.ExtractToDirectory(downloadPath, extractPath);
        logger.Debug("Finished extracting.");
        return Path.Combine(extractPath, "Lib9c.Plugin.dll");
    }
}