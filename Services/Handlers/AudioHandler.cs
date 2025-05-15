using Discord.Interactions;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Rest.Entities.Tracks;

namespace DeusAeon.Services.Handlers;

public class AudioHandler : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<AudioHandler> _logger;
    private readonly IAudioService _audioService;
    
    public AudioHandler(ILogger<AudioHandler> logger, IAudioService audioService)
    {
        _logger = logger;
        _audioService = audioService;
    }

    [SlashCommand("play", "Play selected song", runMode: RunMode.Async)]
    public async Task PlaySongAsync(string link)
    {
        await DeferAsync().ConfigureAwait(false);
        
        var player = await GetPlayerAsync(connectToVoiceChannel: true).ConfigureAwait(false);

        if (player == null)
            return;
        
        var track = await _audioService.Tracks
            .LoadTrackAsync(link, TrackSearchMode.YouTube)
            .ConfigureAwait(false);

        if (track is null)
        {
            await FollowupAsync("No results :(").ConfigureAwait(false);
            return;
        }

        await player.PlayAsync(track).ConfigureAwait(false);
        await FollowupAsync($"Played {track.Title}").ConfigureAwait(false);
        
    }

    private async ValueTask<QueuedLavalinkPlayer?> GetPlayerAsync(bool connectToVoiceChannel = true)
    {
        var channelBehavior = connectToVoiceChannel
            ? PlayerChannelBehavior.Join
            : PlayerChannelBehavior.None;

        var retrieveOptions = new PlayerRetrieveOptions(ChannelBehavior: channelBehavior);

        var result = await _audioService.Players
            .RetrieveAsync(Context, playerFactory: PlayerFactory.Queued, retrieveOptions: retrieveOptions)
            .ConfigureAwait(false);

        if (result.IsSuccess) 
            return result.Player;
        
        var errorMessage = result.Status switch
        {
            PlayerRetrieveStatus.UserNotInVoiceChannel => "You are not connected to a voice channel.",
            PlayerRetrieveStatus.BotNotConnected => "Bot is currently not connected to a voice channel.",
            _ => "Unknown Error"
        };
            
        await FollowupAsync(errorMessage).ConfigureAwait(false);
        return null;
    }

    private Task<bool> ValidateLink(string link)
    {
        return Task.FromResult(true);
    }
}