using EmptyChronicle.Domain.Model.StateTrie;
using EmptyChronicle.Domain.Model.States;
using EmptyChronicle.Infrastructure.StateTrie;
using EmptyChronicle.Infrastructure.States;

namespace EmptyChronicle.Domain;

public static class RepositoryInjectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IStateTrieRepository, LibplanetStateTrieRepository>();
        services.AddSingleton<IStatesRepository, LibplanetStatesRepository>();

        return services;
    }
}
