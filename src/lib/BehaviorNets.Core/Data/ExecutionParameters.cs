using System;

namespace BehaviorNets.Data;

/// <summary>
/// Provides parameters for the examination environment.
/// </summary>
public class ExecutionParameters
{
    /// <summary>
    /// Gets the default execution parameters.
    /// </summary>
    public static ExecutionParameters Default
    {
        get;
    } = new();

    /// <summary>
    /// Gets a value indicating the maximum amount of time that a sample is allowed to execute.
    /// </summary>
    public TimeSpan Timeout
    {
        get;
        set;
    } = TimeSpan.FromMinutes(5);
}