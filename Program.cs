using DeusAeon.OptionBinding;
using DeusAeon.Services;
using DeusAeon.Services.Handlers;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Extensions;
using Lavalink4NET.InactivityTracking.Extensions;
using Lavalink4NET.InactivityTracking.Trackers.Idle;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(
        restrictedToMinimumLevel: LogEventLevel.Debug
        )
    .WriteTo.File("log.log", 
        rollingInterval: RollingInterval.Day,
        rollOnFileSizeLimit: true
        )
    .CreateLogger();

var builder = Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .ConfigureServices(services =>
    {
        var startupConfig = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        services.AddOptions<UserSecretsData>().BindConfiguration("SetupConfigurationSecrets")
            .Services.AddTransient<UserSecretsData>(sp => 
                sp.GetRequiredService<IOptionsMonitor<UserSecretsData>>().CurrentValue
            );
        
        services.AddLavalink();
        services.ConfigureLavalink(config=>
        {
            config.BaseAddress = new Uri($"http://{startupConfig["SetupConfigurationSecrets:LavalinkAdress"]!}");
            config.ReadyTimeout = TimeSpan.FromSeconds(15);
            config.ResumptionOptions = new LavalinkSessionResumptionOptions(TimeSpan.FromSeconds(60));
            config.Passphrase = startupConfig["SetupConfigurationSecrets:LavaLinkPassphrase"]!;
        });
        
        services.AddSingleton<AudioHandler>();
        services.AddSingleton<DiscordSocketClient>();       
        services.AddSingleton<InteractionService>(sp => 
            new InteractionService(sp.GetRequiredService<DiscordSocketClient>())
        );
            
        services.AddHostedService<SlashCommandService>();    
        services.AddHostedService<DiscordStartupService>(); 
    });

await builder.RunConsoleAsync();

    