using System.Collections.Immutable;
using Discord.Interactions;
using Lavalink4NET;
using Lavalink4NET.Clients;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Preconditions;
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
    public async Task PlaySongAsync(string query)
    {
        await DeferAsync().ConfigureAwait(false);
        
        var player = await TryGetPlayerAsync(
            connectToVoiceChannel: true, 
            requireChannel: true
            );

        if (player == null)
            return;
        
        var track = await _audioService.Tracks
            .LoadTrackAsync(query, TrackSearchMode.YouTube)
            .ConfigureAwait(false);
        
        if (track is null)
        { 
            await FollowupAsync("No tracks found :(");
            return;
        }
        await player.PlayAsync(track).ConfigureAwait(false);
        await FollowupAsync($"Start playing: {track.Uri}");
    }

    [SlashCommand("resume", "Resume playing music", runMode: RunMode.Async)]
    public async Task ResumeSongAsync()
    {
        await DeferAsync().ConfigureAwait(false);
        
        var player = await TryGetPlayerAsync(
            connectToVoiceChannel: true, 
            requireChannel: true,
            preconditions: [PlayerPrecondition.Paused]
            );
        
        if (player == null)
            return;
        
        await player.ResumeAsync().ConfigureAwait(false);
        await FollowupAsync($"Resume playing: {player.CurrentTrack!.Uri}");
    }

    [SlashCommand("pause", "Pause playing music", runMode: RunMode.Async)]
    public async Task PauseSongAsync()
    {
        await DeferAsync().ConfigureAwait(false);
        
        var player = await TryGetPlayerAsync(
            connectToVoiceChannel: false,
            preconditions: [PlayerPrecondition.NotPaused]
            );
        
        if (player == null)
            return;
        
        await player.
            PauseAsync()
            .ConfigureAwait(false);
        
        await FollowupAsync($"Track paused");
    }

    [SlashCommand("skip", "Skips the current track", runMode: RunMode.Async)]
    public async Task SkipTrackAsync()
    {
        await DeferAsync().ConfigureAwait(false);

        var player = await TryGetPlayerAsync(
            requireChannel: true,
            preconditions: [PlayerPrecondition.QueueNotEmpty]
        );
        
        if (player == null)
            return;
        
        await player.SkipAsync().ConfigureAwait(false);
        await FollowupAsync($"Skipped. Now playing: {player.CurrentTrack!.Uri}");
    }
    
    private async ValueTask<QueuedLavalinkPlayer?> TryGetPlayerAsync(
        bool connectToVoiceChannel = true,
        bool requireChannel = true,
        ImmutableArray<IPlayerPrecondition> preconditions = default,
        bool isDeferred = false,
        CancellationToken cancellationToken = default
        )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var options = new PlayerRetrieveOptions(
            ChannelBehavior: connectToVoiceChannel ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None,
            VoiceStateBehavior: requireChannel ? MemberVoiceStateBehavior.RequireSame : MemberVoiceStateBehavior.Ignore,
            Preconditions: preconditions);
            
        var result = await _audioService.Players
            .RetrieveAsync(Context, playerFactory: PlayerFactory.Queued, retrieveOptions: options, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (result.IsSuccess) 
            return result.Player;

        var errorMessage = PreconditionErrorHandler.CreateErrorEmbed(result);
            
        await RespondAsync(embed: errorMessage);
        return null;
    }
}