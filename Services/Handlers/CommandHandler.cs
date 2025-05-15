using Discord;
using Discord.Interactions;

namespace DeusAeon.Services.Handlers;

public class CommandHandler : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<CommandHandler> _logger;
    
    public CommandHandler(ILogger<CommandHandler> logger)
    {
        _logger = logger;
    }

    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [SlashCommand("join", "let DeusAeon join your channel", runMode: RunMode.Async)]
    public async Task JoinChannel(IVoiceChannel? channel = null)
    {
        channel ??= (Context.User as IGuildUser)?.VoiceChannel;
        if (channel == null)
        {
            await Context.Interaction.RespondAsync("You must be in a voice channel for me to join.");
        }

        try
        {
            await channel!.ConnectAsync();
            await RespondAsync($"I've joined the channel: {channel.Name}");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to connect to voice channel.");
            await Context.Interaction.RespondAsync("I can't connect to voice channel an error occured.");
        }
    }
}