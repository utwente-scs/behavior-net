namespace BehaviorNets.Data;

/// <summary>
/// Provides information about the result of a single execution of a sample.
/// </summary>
public class ExecutionResult
{
    /// <summary>
    /// Creates a new instance of the <see cref="ExecutionResult"/> class.
    /// </summary>
    /// <param name="hash">The hash of the file that was analyzed, in hexadecimal notation.</param>
    /// <param name="eventStream">The recorded event stream.</param>
    public ExecutionResult(string hash, ExecutionEventStream eventStream)
    {
        Hash = hash;
        EventStream = eventStream;
    }

    /// <summary>
    /// Gets the hash of the sample, in hexadecimal notation.
    /// </summary>
    public string Hash
    {
        get;
    }

    /// <summary>
    /// Gets the event stream that was recorded during the execution of the sample.
    /// </summary>
    public ExecutionEventStream EventStream
    {
        get;
    }
}