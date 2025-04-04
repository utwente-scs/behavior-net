using System;

namespace BehaviorNets.CLI.Analytics;

/// <summary>
/// Models a single entry in a analysis report aggregation.
/// </summary>
public struct ResultsEntry
{
    /// <summary>
    /// Creates a new result entry.
    /// </summary>
    /// <param name="file">The path to the file that was analyzed.</param>
    /// <param name="hash">The hash of the file that was analyzed.</param>
    /// <param name="hasEmptyTrace">Indicates the results were derived from an empty trace.</param>
    public ResultsEntry(string file, string hash, bool hasEmptyTrace)
    {
        File = file;
        Hash = hash;
        DetectedBehaviors = Array.Empty<string>();
        HasEmptyTrace = hasEmptyTrace;
    }
        
    /// <summary>
    /// Creates a new result entry.
    /// </summary>
    /// <param name="file">The path to the file that was analyzed.</param>
    /// <param name="hash">The hash of the file that was analyzed.</param>
    /// <param name="detectedBehaviors">A collection of behaviors that were detected.</param>
    public ResultsEntry(string file, string hash, string[] detectedBehaviors)
    {
        File = file;
        Hash = hash;
        DetectedBehaviors = detectedBehaviors;
        HasEmptyTrace = false;
    }

    /// <summary>
    /// Gets or sets the path to the file that was analyzed.
    /// </summary>
    public string File
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the hash of the file that was analyzed.
    /// </summary>
    public string Hash
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a collection of behaviors that were detected.
    /// </summary>
    public string[] DetectedBehaviors
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating the results were derived from an empty trace.
    /// </summary>
    public bool HasEmptyTrace
    {
        get;
        set;
    }
}