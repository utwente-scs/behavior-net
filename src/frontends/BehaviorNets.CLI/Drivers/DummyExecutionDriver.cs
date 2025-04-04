using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BehaviorNets.Data;
using Serilog;

namespace BehaviorNets.CLI.Drivers;

/// <summary>
/// Provides a dummy execution driver plugin. This is only meant for testing purposes.
/// </summary>
public class DummyExecutionDriver : IExecutionDriver
{
    /// <inheritdoc />
    public Task<ExecutionResult> ExecuteFileAsync(ILogger logger, string path, ExecutionParameters parameters)
    {
        logger.Debug("Dummy analyze sample {Path} with parameters {@Parameters}", path, parameters);
        return Task.FromResult(new ExecutionResult(
            Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))),
            new ExecutionEventStream()));
    }

    /// <inheritdoc />
    public Task<ExecutionResult> ReadTraceAsync(ILogger logger, string path)
    {
        logger.Debug("Dummy read trace {Path}", path);
        return Task.FromResult(new ExecutionResult(
            Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))),
            new ExecutionEventStream()));
    }

    /// <inheritdoc />
    public IEnumerable<string> LocateLogPaths(string path) => Enumerable.Empty<string>();

    /// <inheritdoc />
    void IDisposable.Dispose()
    {
    }
}