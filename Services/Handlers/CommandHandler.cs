using Discord.Interactions;

namespace DeusAeon.Services.Handlers;

public class CommandHandler : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<CommandHandler> _logger;
    
    public CommandHandler(ILogger<CommandHandler> logger)
    {
        _logger = logger;
    }
    
    
}