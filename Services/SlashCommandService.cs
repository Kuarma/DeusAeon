using System.Reflection;
using Discord.Interactions;
using Discord.WebSocket;

namespace DeusAeon.Services;

public class SlashCommandService : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interaction;
    private readonly IServiceProvider _services;
    private readonly ILogger<SlashCommandService> _logger;
    
    public SlashCommandService(ILogger<SlashCommandService> logger, IServiceProvider services, InteractionService interaction, DiscordSocketClient client)
    {
        _logger = logger;
        _services = services;
        _interaction = interaction;
        _client = client;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _client.Ready += () => _interaction.RegisterCommandsGloballyAsync(true);
        _client.InteractionCreated += OnInteractionCreatedAsync;

        await _interaction.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _interaction.Dispose();
        return Task.CompletedTask;
    }
    
    private async Task OnInteractionCreatedAsync(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(_client, interaction);
            var result = await _interaction.ExecuteCommandAsync(context, _services);

            if (!result.IsSuccess)
            {
                await context.Channel.SendMessageAsync("Something went wrong.");
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occured while processing a interaction.");
            await interaction.RespondAsync("Something went wrong.");
        }
    }
}