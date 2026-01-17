using k8s;
using Microsoft.Extensions.Logging;

namespace K8sOperator.NET;

public static partial class LoggerExtensions
{

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "No controllers added, stopping operator."
    )]
    public static partial void NoWatchers(this ILogger logger);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Error watching {ns}/{resource} {label}")]
    public static partial void LogWatchError(this ILogger logger, Exception? ex, string ns, string resource, string label);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "Stop Operator."
    )]
    public static partial void StopOperator(this ILogger logger);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "Begin watch {plural} {labelselector}"
    )]
    public static partial void BeginWatch(this ILogger logger, string plural, string labelselector);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "End watch {plural} {labelselector}"
    )]
    public static partial void EndWatch(this ILogger logger, string plural, string labelselector);

    [LoggerMessage(
       EventId = 6,
       Level = LogLevel.Debug,
       Message = "Event: '{EventType}' received for resource: {Resource}"
   )]
    public static partial void EventReceived(this ILogger logger, WatchEventType eventType, CustomResource resource);


    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Error,
        Message = "Error processing {EventType} {Resource}"
        )]
    public static partial void ProcessEventError(this ILogger logger, Exception? exception, WatchEventType eventType, CustomResource resource);

    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Debug,
        Message = "Handle Delete {resource}")]
    public static partial void HandleDelete(this ILogger logger, CustomResource resource);

    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Debug,
        Message = "Begin Delete {resource}")]
    public static partial void BeginDelete(this ILogger logger, CustomResource resource);

    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Debug,
        Message = "Skip Delete {resource}")]
    public static partial void SkipDelete(this ILogger logger, CustomResource resource);

    [LoggerMessage(
        EventId = 11,
        Level = LogLevel.Debug,
        Message = "End Delete {resource}")]
    public static partial void EndDelete(this ILogger logger, CustomResource resource);

    [LoggerMessage(
        EventId = 12,
        Level = LogLevel.Debug,
        Message = "Replace Resource {resource}")]
    public static partial void ReplaceResource(this ILogger logger, CustomResource resource);

    [LoggerMessage(
        EventId = 13,
        Level = LogLevel.Debug,
        Message = "Begin Add or Modify {resource}")]
    public static partial void HandleAddOrModify(this ILogger logger, CustomResource resource);
    [LoggerMessage(
        EventId = 14,
        Level = LogLevel.Debug,
        Message = "Add finalizer to resource {resource}")]
    public static partial void AddFinalizer(this ILogger logger, CustomResource resource);

    [LoggerMessage(
        EventId = 15,
        Level = LogLevel.Debug,
        Message = "Skip Add or Modify {resource} already handled")]
    public static partial void SkipAddOrModify(this ILogger logger, CustomResource resource);

    [LoggerMessage(
        EventId = 16,
        Level = LogLevel.Debug,
        Message = "End Add or Modify {resource} already handled")]
    public static partial void EndAddOrModify(this ILogger logger, CustomResource resource);

    [LoggerMessage(
        EventId = 17,
        Level = LogLevel.Debug,
        Message = "Start Finalize {resource}")]
    public static partial void HandleFinalize(this ILogger logger, CustomResource resource);

    [LoggerMessage(
        EventId = 18,
        Level = LogLevel.Debug,
        Message = "Skip Finalize {resource}")]
    public static partial void SkipFinalize(this ILogger logger, CustomResource resource);

    [LoggerMessage(
        EventId = 19,
        Level = LogLevel.Debug,
        Message = "Begin Finalize {resource}")]
    public static partial void BeginFinalize(this ILogger logger, CustomResource resource);

    [LoggerMessage(
        EventId = 20,
        Level = LogLevel.Debug,
        Message = "End Finalize {resource}")]
    public static partial void EndFinalize(this ILogger logger, CustomResource resource);

    [LoggerMessage(
        EventId = 21,
        Level = LogLevel.Debug,
        Message = "Remove Finalizer from {resource}")]
    public static partial void RemoveFinalizer(this ILogger logger, CustomResource resource);

    [LoggerMessage(
        EventId = 22,
        Level = LogLevel.Debug,
        Message = "Handle Bookmark {resource}")]
    public static partial void HandleBookmark(this ILogger logger, CustomResource resource);

    [LoggerMessage(
        EventId = 23,
        Level = LogLevel.Debug,
        Message = "Begin Bookmark {resource}")]
    public static partial void BeginBookmark(this ILogger logger, CustomResource resource);

    [LoggerMessage(
        EventId = 24,
        Level = LogLevel.Debug,
        Message = "End Bookmark {resource}")]
    public static partial void EndBookmark(this ILogger logger, CustomResource resource);

    [LoggerMessage(
        EventId = 25,
        Level = LogLevel.Debug,
        Message = "Handle Error {resource}")]
    public static partial void HandleError(this ILogger logger, CustomResource resource);

    [LoggerMessage(
        EventId = 26,
        Level = LogLevel.Debug,
        Message = "Begin Error {resource}")]
    public static partial void BeginError(this ILogger logger, CustomResource resource);

    [LoggerMessage(
        EventId = 27,
        Level = LogLevel.Debug,
        Message = "End Error {resource}")]
    public static partial void EndError(this ILogger logger, CustomResource resource);

    [LoggerMessage(
        EventId = 28,
        Level = LogLevel.Information,
        Message = "Watcher Error {message}")]
    public static partial void WatcherError(this ILogger logger, string message);

    [LoggerMessage(
        EventId = 29,
        Level = LogLevel.Information,
        Message = "WatchAsync {ns}/{plural} {labelselector}"
    )]
    public static partial void WatchAsync(this ILogger logger, string ns, string plural, string labelselector);
}
