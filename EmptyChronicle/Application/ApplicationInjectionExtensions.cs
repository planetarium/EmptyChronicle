using EmptyChronicle.Application.StateTrie;
using EmptyChronicle.Application.States;

namespace EmptyChronicle.Application;

public static class ApplicationInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<StateTrieApplication>();
        services.AddSingleton<StatesApplication>();

        return services;
    }
}
