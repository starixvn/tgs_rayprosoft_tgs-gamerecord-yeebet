using System.Reflection;
using Tgs.GameCommand.Client;
using Tgs.GameInterface.YeeBet.Client;
using Tgs.GameRecord.YeeBet.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameRecordCore(
        this IServiceCollection services,
        Action<RedisOptions, IServiceProvider> redisDBconfigure,
        Action<GameRecordInfoOptions, IServiceProvider> gameRecordInfoOptions,        
        Action<GameCommandClientOptions, IServiceProvider> gameCommandClientOptions,        
        Action<GameInterfaceYeeBetClientOptions, IServiceProvider> gameInterfaceDreamGamingClientOptions)
        => services
            .AddOptions<RedisOptions>()
            .Configure(redisDBconfigure)
            .Services
            .AddOptions<GameRecordInfoOptions>()
            .Configure(gameRecordInfoOptions)            
            .Services
            .RegisterAllServiceUtilityTypes(
                Assembly.GetExecutingAssembly(),
                ServiceLifetime.Transient)
            .AddMemoryCache()
            .AddSingleton<RedisConnectionManager>()
            .AddGameCommandClient(gameCommandClientOptions)            
            .AddGameInterfaceYeeBetClient(gameInterfaceDreamGamingClientOptions)
            .AddGameRoundsWorkClient()
            .AddGameRoundsWorkManualClient()
            .AddTransient<ISystemClock, SystemClock>()
            .AddTransient<IScheduleDispatch>(sp => ActivatorUtilities.CreateInstance<ScheduleDispatch>(sp, sp.GetRequiredService<RedisConnectionManager>().Database))
            .AddTransient<IUtensil>(sp => ActivatorUtilities.CreateInstance<UtensilRepository>(sp, TimeSpan.FromHours(1)));
}
