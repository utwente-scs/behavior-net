using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BehaviorNets.Data;
using BehaviorNets.Drivers.Drakvuf.Api;
using BehaviorNets.Drivers.Drakvuf.Reporting;
using Serilog;

namespace BehaviorNets.Drivers.Drakvuf;

public class DrakvufExecutionDriver : IExecutionDriver
{
    private readonly DrakvufApiClient _client;

    public DrakvufExecutionDriver()
    {
        string host = Environment.GetEnvironmentVariable("DRAKVUF_HOST");
        if (string.IsNullOrEmpty(host))
            host = "http://localhost:6300/";

        Log.Debug("Using DRAKVUF host {Host}.", host);

        _client = new DrakvufApiClient(new Uri(host));
    }

    public DrakvufExecutionDriver(Uri baseUri)
    {
        _client = new DrakvufApiClient(baseUri);
    }

    public bool IncludeApiMonLog
    {
        get;
        set;
    } = true;

    public bool IncludeSyscallLog
    {
        get;
        set;
    } = false;

    public bool IncludeRegMonLog
    {
        get;
        set;
    } = true;

    public bool IncludeLibraryMonLog
    {
        get;
        set;
    } = true;

    /// <inheritdoc />
    public async Task<ExecutionResult> ExecuteFileAsync(
        ILogger logger, 
        string path,
        ExecutionParameters parameters)
    {
        var drakvufParameters = CreateDrakvufParameters(path, parameters);
        logger.Debug("Enabled plugins: {@Plugins}", drakvufParameters.EnabledPlugins);

        string taskUid = await _client.CreateTaskAsync(drakvufParameters);
        logger.Debug("New DRAKVUF task ID: {TaskUID}.", taskUid);

        var start = DateTime.Now;

        logger.Debug("Waiting for execution to complete...", taskUid);
        while (true)
        {
            await Task.Delay(10000);
            var info = await _client.GetTaskStatusAsync(taskUid);
            if (info is not null && info.StatusCode == TaskStatusCode.Done)
                break;
        }

        var end = DateTime.Now;
        if (end - start < parameters.Timeout)
            Log.Warning("Server reported task was done prior to the specified timeout.");

        return await DownloadAndCreateEventStream(logger, taskUid);
    }

    private TaskCreateParameters CreateDrakvufParameters(string path, ExecutionParameters parameters)
    {
        var drakvufParameters = new TaskCreateParameters
        {
            FilePath = path,
            Timeout = (int)parameters.Timeout.TotalMinutes
        };

        var plugins = new List<string>();

        if (IncludeApiMonLog)
            plugins.Add("apimon");
        if (IncludeSyscallLog)
            plugins.Add("syscall");
        if (IncludeRegMonLog)
            plugins.Add("regmon");
        if (IncludeLibraryMonLog)
            plugins.Add("librarymon");

        drakvufParameters.EnabledPlugins = plugins.ToArray();
        return drakvufParameters;
    }

    private async Task<ExecutionResult> DownloadAndCreateEventStream(ILogger logger, string taskUid)
    {
        logger.Debug("Downloading drakrun.log...");
        var drakRunReport = await _client.GetDrakrunReport(taskUid);
            
        ApiMonReport apiMonReport = null;
        SysCallReport sysCallReport = null;
        RegMonReport regMonReport = null;
        LibraryMonReport libraryMonReport = null;
            
        if (IncludeApiMonLog)
        {
            logger.Debug("Downloading apimon.log...");
            apiMonReport = await _client.GetApiMonReport(taskUid);
        }

        if (IncludeSyscallLog)
        {
            logger.Debug("Downloading syscall.log...");
            sysCallReport = await _client.GetSysCallReport(taskUid);
        }

        if (IncludeRegMonLog)
        {
            logger.Debug("Downloading regmon.log...");
            regMonReport = await _client.GetRegMonReport(taskUid);
        }

        if (IncludeLibraryMonLog)
        {
            logger.Debug("Downloading librarymon.log...");
            libraryMonReport = await _client.GetLibraryMonReport(taskUid);
        }

        logger.Debug("Flattening logs into core models...");
        return DrakvufExecutionTraceReader.FromReports(drakRunReport, apiMonReport, sysCallReport, regMonReport, libraryMonReport);
    }

    /// <inheritdoc />
    public Task<ExecutionResult> ReadTraceAsync(ILogger logger, string path)
    {
        if (Directory.Exists(path))
            return Task.FromResult(DrakvufExecutionTraceReader.FromDirectory(path));

        string apimonPath = path;
        string drakrunPath = apimonPath.Replace("apimon", "drakrun");
        string syscallPath = apimonPath.Replace("apimon", "syscall");
        string regmonPath = apimonPath.Replace("apimon", "regmon");
        string libraryMonPath = apimonPath.Replace("apimon", "librarymon");

        if (!File.Exists(drakrunPath))
            drakrunPath = null;
        if (!File.Exists(apimonPath))
            apimonPath = null;
        if (!File.Exists(syscallPath))
            syscallPath = null;
        if (!File.Exists(regmonPath))
            regmonPath = null;
        if (!File.Exists(libraryMonPath))
            libraryMonPath = null;

        return Task.FromResult(DrakvufExecutionTraceReader.FromFiles(
            drakrunPath,
            apimonPath,
            syscallPath,
            regmonPath,
            libraryMonPath));
    }

    /// <inheritdoc />
    public IEnumerable<string> LocateLogPaths(string path)
    {
        var agenda = new Stack<string>();
        agenda.Push(path);

        while (agenda.TryPop(out string current))
        {
            if (File.Exists(Path.Combine(current, "apimon.log")))
            {
                yield return current;
            }
            else
            {
                foreach (string baseDirectory in Directory.GetDirectories(current))
                    agenda.Push(baseDirectory);
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client?.Dispose();
    }
}