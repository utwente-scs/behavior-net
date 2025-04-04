using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BehaviorNets.Data;

/// <summary>
/// Represents a single recorded event in an event stream.
/// </summary>
public class ExecutionEvent
{
    /// <summary>
    /// Creates a new instance of a recorded event.
    /// </summary>
    /// <param name="time">The unix timestamp of the event.</param>
    /// <param name="name">The name of the event that was observed.</param>
    public ExecutionEvent(decimal time, string name)
    {
        Time = time;
        Name = name;
        Arguments = new List<object?>();
    }

    /// <summary>
    /// Creates a new instance of a recorded event.
    /// </summary>
    /// <param name="time">The unix timestamp of the event.</param>
    /// <param name="name">The name of the event that was observed.</param>
    /// <param name="arguments">The arguments that were passed onto the event.</param>
    public ExecutionEvent(decimal time, string name, params object?[] arguments)
    {
        Time = time;
        Name = name;
        Arguments = new List<object?>(arguments);
    }

    /// <summary>
    /// Creates a new instance of a recorded event.
    /// </summary>
    /// <param name="time">The unix timestamp of the event.</param>
    /// <param name="name">The name of the event that was observed.</param>
    /// <param name="processId">The ID of the process that produced the event.</param>
    /// <param name="threadId">The ID of the thread that produced teh event.</param>
    public ExecutionEvent(decimal time, uint processId, uint threadId, string name)
    {
        Time = time;
        ProcessId = processId;
        ThreadId = threadId;
        Name = name;
        Arguments = new List<object?>();
    }

    /// <summary>
    /// Creates a new instance of a recorded event.
    /// </summary>
    /// <param name="time">The unix timestamp of the event.</param>
    /// <param name="name">The name of the event that was observed.</param>
    /// <param name="processId">The ID of the process that produced the event.</param>
    /// <param name="threadId">The ID of the thread that produced teh event.</param>
    /// <param name="arguments">The arguments that were passed onto the event.</param>
    public ExecutionEvent(decimal time, uint processId, uint threadId, string name, params object?[] arguments)
    {
        Time = time;
        ProcessId = processId;
        ThreadId = threadId;
        Name = name;
        Arguments = new List<object?>(arguments);
    }

    /// <summary>
    /// Gets the unix timestamp when the event was recorded.
    /// </summary>
    public decimal Time
    {
        get;
    }

    /// <summary>
    /// Gets the ID of the process responsible for producing the event.
    /// </summary>
    public uint ProcessId
    {
        get;
    }

    /// <summary>
    /// Gets the ID of the thread responsible for producing the event.
    /// </summary>
    public uint ThreadId
    {
        get;
    }

    /// <summary>
    /// Gets the name of the event that was recorded.
    /// </summary>
    public string Name
    {
        get;
    }

    /// <summary>
    /// Gets the arguments that were passed onto the event.
    /// </summary>
    public IList<object?> Arguments
    {
        get;
    }

    /// <summary>
    /// Gets the return value of the event.
    /// </summary>
    public object? ReturnValue
    {
        get;
        set;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append(Time.ToString(CultureInfo.InvariantCulture));
        builder.Append(": ");
        builder.Append(Name);
        builder.Append('(');

        for (int i = 0; i < Arguments.Count; i++)
        {
            builder.AppendFormat("{0:X}", Arguments[i]);
            if (i < Arguments.Count - 1)
                builder.Append(", ");
        }

        builder.Append(')');

        if (ReturnValue is not null)
        {
            builder.Append(" -> ");
            builder.Append(ReturnValue);
        }

        return builder.ToString();
    }

}