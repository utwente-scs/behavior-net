using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;

namespace BehaviorNets.Data;

/// <summary>
/// Provides members for dynamically analyzing samples or loading execution event streams from files.
/// </summary>
public interface IExecutionDriver : IDisposable
{
    /// <summary>
    /// Executes the provided file in the examination environment and records a stream of events that occur
    /// during the execution.
    /// </summary>
    /// <param name="logger">The logger instance to use.</param>
    /// <param name="path">The path of the sample to execute.</param>
    /// <param name="parameters">The execution parameters.</param>
    /// <returns>The recorded event stream.</returns>
    Task<ExecutionResult> ExecuteFileAsync(ILogger logger, string path, ExecutionParameters parameters);

    /// <summary>
    /// Reads an event stream from the disk.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="path">The path to the event stream.</param>
    /// <returns>The read event stream.</returns>
    Task<ExecutionResult> ReadTraceAsync(ILogger logger, string path);

    /// <summary>
    /// Locates files in a directory that make up an event stream.
    /// </summary>
    /// <param name="path">The root directory to start searching.</param>
    /// <returns>The paths to the files that make up the event streams.</returns>
    IEnumerable<string> LocateLogPaths(string path);
}