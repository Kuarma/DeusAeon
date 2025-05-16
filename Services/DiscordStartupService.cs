using DeusAeon.LoggingSetup;
using Discord;
using Discord.WebSocket;

namespace DeusAeon.Services;

public class DiscordStartupService : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly ILogger<DiscordStartupService> _logger;
    private readonly IConfiguration _config;
    
    public DiscordStartupService(DiscordSocketClient client, ILogger<DiscordStartupService> logger, IConfiguration config)
    {
        _client = client;
        _config = config;
        _logger = logger;
        
        _client.Log += DiscordSerilogWrapper.LogAsync;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _client.LoginAsync(TokenType.Bot, _config["DiscordBotToken:Token"]);
        await _client.StartAsync();
        _logger.LogInformation("Discord bot started");
    }
    
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.LogoutAsync();
        _logger.LogInformation("Discord bot stopped");
    }
}
