using Discord;
using Serilog;
using Serilog.Events;

namespace DeusAeon.LoggingSetup;

public static class DiscordSerilogWrapper
{
    public static async Task LogAsync(LogMessage logMessage)
    {
        var severity = logMessage.Severity switch
        {
            LogSeverity.Critical => LogEventLevel.Fatal,
            LogSeverity.Error => LogEventLevel.Error,
            LogSeverity.Warning => LogEventLevel.Warning,
            LogSeverity.Info => LogEventLevel.Information,
            LogSeverity.Verbose => LogEventLevel.Verbose,
            LogSeverity.Debug => LogEventLevel.Debug,
            _ => LogEventLevel.Information
        };
        
        Log.Write(severity, logMessage.Exception, "[{Source}] {Message}", logMessage.Source, logMessage.Message);
        await Task.CompletedTask;
    }
}