using System.Collections.Generic;

namespace BehaviorNets.Data;

/// <summary>
/// Represents a stream of recorded events.
/// </summary>
public class ExecutionEventStream
{
    /// <summary>
    /// Creates a new empty event stream.
    /// </summary>
    public ExecutionEventStream()
    {
        Events = new List<ExecutionEvent>();
    }

    /// <summary>
    /// Creates a new event stream based on the provided list of recorded events.
    /// </summary>
    /// <param name="events"></param>
    public ExecutionEventStream(IEnumerable<ExecutionEvent> events)
    {
        Events = new List<ExecutionEvent>(events);
    }

    /// <summary>
    /// Gets the recorded events in this stream.
    /// </summary>
    public IList<ExecutionEvent> Events
    {
        get;
    }
}