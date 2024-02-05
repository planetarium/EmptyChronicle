using EmptyChronicle.Domain.Model.StateTrie;
using EmptyChronicle.Infrastructure.StateTrie;

namespace EmptyChronicle.Domain;

public static class RepositoryInjectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IStateTrieRepository, LibplanetStateTrieRepository>();

        return services;
    }
}
