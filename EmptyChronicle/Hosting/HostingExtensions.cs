using System.Collections.Immutable;
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
using Libplanet.RocksDBStore;
using Libplanet.Common;
using Libplanet.Action.State;
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
            .AddSingleton<IBlockPolicy>(_ => new BlockPolicySource(Log.Logger).GetPolicy())
            .AddSingleton<IStagePolicy>(_ => new VolatileStagePolicy())
            .AddSingleton<IStore>(_ => new RocksDBStore(configuration.StorePath))
            .AddSingleton<IStateStore>(_ => new TrieStateStore(
                new RocksDBKeyValueStore(Path.Combine(configuration.StorePath ?? "planet-node-chain", "states"))
            ))
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
            .AddSingleton<IActionEvaluator>(provider => new ActionEvaluator(
                _ => provider.GetRequiredService<IBlockPolicy>().BlockAction,
                provider.GetRequiredService<IBlockChainStates>(),
                provider.GetRequiredService<IActionLoader>()
            ))
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
}