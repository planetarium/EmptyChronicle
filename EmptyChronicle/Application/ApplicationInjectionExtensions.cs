using EmptyChronicle.Application.StateTrie;

namespace EmptyChronicle.Application;

public static class ApplicationInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<StateTrieApplication>();

        return services;
    }
}