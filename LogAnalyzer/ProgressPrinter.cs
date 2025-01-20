using System.Runtime.CompilerServices;

namespace LogAnalyzer;

/// <summary>
/// Helper class that allow to avoid some boilerplate code for saving task progress  
/// </summary>
public class ProgressPrinter
{
    public string TaskName { get; private set; }
    public float LastLoggedProgress;
    public float LogInterval;
    public DateTime StartTime;
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="taskName">Task name</param>
    /// <param name="logInterval">Interval of printing intermediate progress</param>
    public ProgressPrinter(string taskName, float logInterval)
    {
        TaskName = taskName;
        StartTime = DateTime.Now;
        LogInterval = logInterval;
        Console.WriteLine($"\nTask '{taskName}' started at {DateTime.Now}\n");
    }

    public void Finish(string? customMessage = null)
    {
        Console.WriteLine($"\nTask '{TaskName}' finished at {DateTime.Now}, spent: {DateTime.Now - StartTime}\n");
        if(customMessage is not null)
            Console.WriteLine(customMessage);
        Console.WriteLine("");
    }

    /// <summary>
    /// Print intermediate progress of the task
    /// </summary>
    /// <param name="progress">The value of current progress having the same unit as logInterval given in the constructor</param>
    /// <param name="progressDetails">Additional string information to print</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PrintProgress(float progress, string? progressDetails = null)
    {
        if (progress >= LastLoggedProgress + LogInterval)
        {
            var elapsedTime = DateTime.Now - StartTime;

            var estimatedTotalTime = progress > 0 
                ? TimeSpan.FromTicks((long)(elapsedTime.Ticks / progress * 100))
                : TimeSpan.Zero;

            var remainingTime = estimatedTotalTime - elapsedTime;

            string FormatTimeSpan(TimeSpan timeSpan) => $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";

            var progressMessage = progressDetails is null
                ? $"progress: {progress}%"
                : $"progress: {progress}% ({progressDetails})";

            Console.WriteLine(
                $"'{TaskName}' {progressMessage}, time: {DateTime.Now:HH:mm:ss}, spent: {FormatTimeSpan(elapsedTime)}, estimated remaining: {FormatTimeSpan(remainingTime)}, estimated total: {FormatTimeSpan(estimatedTotalTime)}");
            
            LastLoggedProgress = progress;
        }
    }
}