using Discord;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Preconditions;
using Lavalink4NET.Players.Queued;

namespace DeusAeon.Services.Handlers;

public static class PreconditionErrorHandler
{
    public static Embed CreateErrorEmbed(PlayerResult<QueuedLavalinkPlayer> result)
    {
        var title = result.Status switch
        {
            PlayerRetrieveStatus.UserNotInVoiceChannel => "You must be in a voice channel.",
            PlayerRetrieveStatus.BotNotConnected => "The bot is not connected to any channel.",
            PlayerRetrieveStatus.VoiceChannelMismatch => "You must be in the same voice channel as the bot.",

            PlayerRetrieveStatus.PreconditionFailed when Equals(result.Precondition, PlayerPrecondition.Playing) => "The player is currently now playing any track.",
            PlayerRetrieveStatus.PreconditionFailed when Equals(result.Precondition, PlayerPrecondition.NotPaused) => "The player is already paused.",
            PlayerRetrieveStatus.PreconditionFailed when Equals(result.Precondition, PlayerPrecondition.Paused) => "The player is not paused.",
            PlayerRetrieveStatus.PreconditionFailed when Equals(result.Precondition, PlayerPrecondition.QueueEmpty) => "The queue is empty.",

            _ => "Unknown error.",
        };

        return new EmbedBuilder().WithTitle(title).Build();
    }
}