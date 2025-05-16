using DeusAeon.Services;
using DeusAeon.Services.Handlers;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Extensions;
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
        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        
        services.AddLavalink();
        services.ConfigureLavalink(config=>
        {
            config.BaseAddress = new Uri($"http://{configuration["Lavalink:Ip"]!}");
            config.ReadyTimeout = TimeSpan.FromSeconds(15);
            config.ResumptionOptions = new LavalinkSessionResumptionOptions(TimeSpan.FromSeconds(60));
            config.Passphrase = configuration["Lavalink:Passphrase"]!;
        });
        
        services.AddSingleton<AudioHandler>();
        services.AddSingleton<CommandHandler>();
        services.AddSingleton<DiscordSocketClient>();       
        services.AddSingleton<InteractionService>(sp => 
            new InteractionService(sp.GetRequiredService<DiscordSocketClient>())
        );
            
        services.AddHostedService<SlashCommandService>();    
        services.AddHostedService<DiscordStartupService>(); 
    });

await builder.RunConsoleAsync();

    