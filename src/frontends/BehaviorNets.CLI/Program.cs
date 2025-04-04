﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BehaviorNets.CLI.Analytics;
using BehaviorNets.CLI.Drivers;
using BehaviorNets.Data;
using BehaviorNets.Parser;
using Serilog;
using Serilog.Events;

namespace BehaviorNets.CLI;

/// <summary>
/// The main program entry point of the analyzer.
/// </summary>
internal static class Program
{
    private const string LogFormat = "{Timestamp:yy-MM-dd HH:mm:ss} {Level:u3} [{Progress,3:0}%] <Worker{WorkerId}>: {Message}{NewLine}";

    private static readonly DriverRepository DriverRepository;
    private static readonly string ProgramDirectory;
    private static readonly ExecutionEventStreamAnalyzer Analyzer;
    private static readonly ProgressEnricher ProgressEnricher = new();
    private static IExecutionDriver _driver;
    private static IResultsWriter _resultsWriter;

    static Program()
    {
        // Set up driver plugin repository.
        ProgramDirectory = Path.GetDirectoryName(typeof(Program).Assembly.Location)!;
        string driversDirectory = Path.Combine(ProgramDirectory, "Drivers");
        DriverRepository = new DriverRepository(
            driversDirectory,
            Path.Combine(driversDirectory, "drivers.json"));

        // Set up default services.
        Analyzer = new ExecutionEventStreamAnalyzer();
        _driver = new DummyExecutionDriver();
        _resultsWriter = EmptyResultsWriter.Instance;
    }

    /// <summary>
    /// The main entry point of the application.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    public static void Main(string[] args)
    {
        var analyzeCommand = new CommandBuilder(new Command("analyze", "Run a sample in a virtual environment and analyze its behavior."))
            .AddArgument(new Argument<string[]>("paths"))
            .AddOption(new Option("--dump-marking", "Dumps the final markings of all behavior nets."))
            .AddOption(new Option<int>(new[] {"-et", "--execution-timeout"}, () => 3, "Specifies the time-out of the execution in minutes."))
            .AddOption(new Option<int>(new[] {"-at", "--analysis-timeout"}, () => 3, "Specifies the time-out of the analysis in minutes."))
            .Command;

        analyzeCommand.Handler = CommandHandler.Create<string[], string, string?, bool, bool, bool, string?, int, int, int, bool>(HandleAnalyzeCommand);

        var logAnalyzeCommand = new CommandBuilder(new Command("log-analyze", "Analyze a log file for behaviors."))
            .AddArgument(new Argument("paths"))
            .AddOption(new Option("--dump-marking", "Dumps the final markings of all behavior nets."))
            .AddOption(new Option("--graphviz", "Output GraphViz representations of all installed behaviors."))
            .AddOption(new Option<int>(new[] {"-at", "--analysis-timeout"}, () => 3, "Specifies the time-out of the analysis in minutes."))
            .Command;

        logAnalyzeCommand.Handler = CommandHandler.Create<string[], string, string?, bool, bool, bool, string?, int, int, bool>(HandleLogAnalyzeCommand);

        var listBehaviorsCommand = new CommandBuilder(new Command("list-behaviors",
                "Lists all available behaviors that are installed and can be recognized."))
            .AddOption(new Option("--graphviz", "Output GraphViz representations of all installed behaviors."))
            .Command;

        listBehaviorsCommand.Handler = CommandHandler.Create<bool, bool>(HandleListBehaviorsCommand);

        var command = new CommandBuilder(new RootCommand())
            .AddCommand(analyzeCommand)
            .AddCommand(logAnalyzeCommand)
            .AddCommand(listBehaviorsCommand)
            .AddGlobalOption(new Option<string>("--driver", () => "drakvuf", "Specifies the backend driver to use."))
            .AddGlobalOption(new Option<string>("--log-file", "Specifies the log output file."))
            .AddGlobalOption(new Option(new[] {"-v", "--verbose"}, "Use verbose logging output."))
            .AddGlobalOption(new Option(new[] {"-r", "--recursive"}, "Specifies the provided paths should be traversed recursively for finding files to analyze."))
            .AddGlobalOption(new Option<string?>(new[] {"-o", "--output-file"}, "Specifies the file path to write the results to."))
            .AddGlobalOption(new Option<int>(new[] {"-p", "--parallel"}, () => 1, "Specifies the number of workers that should be active in parallel."))
            .AddGlobalOption(new Option("--dry-run", "Performs a run without actually analyzing anything."))
            .Command;

        try
        {
            command.Invoke(args);
        }
        finally
        {
            _driver.Dispose();
            _resultsWriter.Dispose();
        }
    }

    /// <summary>
    /// Handles the <c>analyze</c> command.
    /// </summary>
    private static void HandleAnalyzeCommand(
        string[] paths,
        string driver,
        string? logFile,
        bool verbose, 
        bool recursive, 
        bool dumpMarking, 
        string? outputFile, 
        int executionTimeout, 
        int analysisTimeout,
        int parallel, 
        bool dryRun)
    {
        ConfigureTool(logFile, verbose, driver, outputFile);
            
        // Get all executable files to analyze.
        var files = GetFilesToAnalyze(paths, recursive, "*.exe");
            
        // Determine which files were already analyzed.
        if (!string.IsNullOrEmpty(outputFile) && File.Exists(outputFile))
            RemoveAlreadyAnalyzedFiles(files, outputFile);

        // Set up execution parameters for the VM task.
        var parameters = new ExecutionParameters
        {
            Timeout = TimeSpan.FromMinutes(executionTimeout)
        };

        if (!dryRun)
        {
            RunParallelizedTasks(files, parallel, dumpMarking, TimeSpan.FromMinutes(analysisTimeout),
                (logger, file) => _driver.ExecuteFileAsync(logger, file, parameters));
        }
    }

    /// <summary>
    /// Handles the <c>log-analyze</c> command.
    /// </summary>
    private static void HandleLogAnalyzeCommand(
        string[] paths,
        string driver,
        string? logFile, 
        bool verbose,
        bool recursive, 
        bool dumpMarking, 
        string? outputFile, 
        int analysisTimeout,
        int parallel,
        bool dryRun)
    {
        ConfigureTool(logFile, verbose, driver, outputFile);

        // Get all log files to analyze.
        ICollection<string> files;
        if (recursive)
        {
            files = paths.SelectMany(p => _driver.LocateLogPaths(p)).ToArray();
            if (files.Count == 0)
                files = GetFilesToAnalyze(paths, true, "*.log");
        }
        else
        {
            files = GetFilesToAnalyze(paths, false, "*.log");
        }

        if (!dryRun)
        {
            RunParallelizedTasks(files, parallel, dumpMarking, TimeSpan.FromMinutes(analysisTimeout),
                (logger, file) => _driver.ReadTraceAsync(logger, file));
        }
    }

    /// <summary>
    /// Handles the <c>list-behaviors</c> command.
    /// </summary>
    private static void HandleListBehaviorsCommand(bool verbose, bool graphviz)
    {
        ConfigureLogger(null, verbose);
        CompileBehaviors();

        foreach (var behavior in Analyzer.Behaviors)
        {
            if (graphviz)
                behavior.ToGraphViz(Console.Out);
            else
                Console.WriteLine(behavior.Name);
        }
    }

    /// <summary>
    /// Collects all files to analyze in the provided file or directory paths.
    /// </summary>
    /// <param name="paths">The path to the files or directories to analyze.</param>
    /// <param name="recursive">
    /// Indicates whether the paths in <paramref name="paths"/> should be interpreted as directories and should be
    /// traversed recursively for finding candidate files.
    /// </param> 
    /// <param name="pattern">The file glob pattern to use for matching file paths.</param>
    /// <returns>The total list of files to analyze.</returns>
    private static ICollection<string> GetFilesToAnalyze(string[] paths, bool recursive, string pattern)
    {
        if (!recursive)
            return paths.ToHashSet();

        return paths
            .SelectMany(p => Directory.EnumerateFiles(p, pattern, SearchOption.AllDirectories))
            .ToHashSet();
    }

    /// <summary>
    /// Determines which files were already analyzed based on a summary log file, and removes them from a collection
    /// of candidate samples to analyze. 
    /// </summary>
    /// <param name="files">The collection of samples to filter.</param>
    /// <param name="outputFile">The summary to find already analyzed files in.</param>
    private static void RemoveAlreadyAnalyzedFiles(ICollection<string> files, string outputFile)
    {
        int previousCount = files.Count;
            
        Log.Information("Determining previously analyzed samples");
            
        using var reader = new StreamReader(outputFile);
        while (true)
        {
            string? line = reader.ReadLine();
            if (line is null)
                break;

            var entry = JsonSerializer.Deserialize<ResultsEntry>(line);
            files.Remove(entry.File);
        }

        int difference = previousCount - files.Count;
        Log.Information("Excluded {ExcludedFileCount} files from the analysis", difference);
    }

    /// <summary>
    /// Runs the specified task on the provided files distributed over a set amount of worker threads. 
    /// </summary>
    /// <param name="files">The collection of files to process.</param>
    /// <param name="threadCount">The number of threads to distribute the work over.</param>
    /// <param name="dumpMarking">
    /// Indicates whether after every analysis the markings of the behavior nets should be dumped.
    /// </param>
    /// <param name="analysisTimeout">
    /// Specifies the maximum amount of time an analysis of an event stream can take before it gets cut off.
    /// </param>
    /// <param name="getExecutionResult">The task to perform for every file.</param>
    private static void RunParallelizedTasks(
        ICollection<string> files, 
        int threadCount, 
        bool dumpMarking, 
        TimeSpan analysisTimeout, 
        Func<ILogger, string, Task<ExecutionResult>> getExecutionResult)
    {
        if (threadCount <= 0)
            throw new ArgumentException("Number of samples in parallel must be a number greater than 0.");
            
        Log.Information(
            "Started analysis of {FileCount} files distributed over {WorkerCount} workers",
            files.Count,
            threadCount);

        // Set up task queue.
        var taskQueue = new ConcurrentQueue<string>(files);
        int successCount = 0;
        ProgressEnricher.TotalSteps = files.Count;

        // Register CTRL+C interrupt handler.
        var gracefulCancellation = new CancellationTokenSource();
        var forcedCancellation = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            if (!gracefulCancellation.IsCancellationRequested)
            {
                Log.Information("First Interrupt Received");
                Log.Information("Gracefully waiting for all workers to finish current task");
                gracefulCancellation.Cancel();
                e.Cancel = true;
            }
            else if (!forcedCancellation.IsCancellationRequested)
            {
                Log.Information("Second Interrupt Received");
                Log.Information("Forcing all workers to finish current task");
                forcedCancellation.Cancel();
                e.Cancel = true;
            }
        };

        // Create new worker threads.
        var barrier = new Barrier(threadCount + 1);
        var workerThreads = new Thread[threadCount];
        for (int i = 0; i < workerThreads.Length; i++)
        {
            workerThreads[i] = new Thread(async state =>
            {
                var logger = Log.ForContext("WorkerId", state);
                    
                while (!gracefulCancellation.IsCancellationRequested && taskQueue.TryDequeue(out string? file))
                {
                    logger.Information("Executing file {File}...", file);

                    try
                    {
                        var executionResult = await getExecutionResult(logger, file);
                        DoAnalysisAndReportResults(logger, file, executionResult, dumpMarking, analysisTimeout);
                        Interlocked.Increment(ref successCount);
                    }
                    catch (Exception ex) when (!Debugger.IsAttached)
                    {
                        logger.Error(ex, "Failed to analyze {File}: {Error}", file, ex.ToString());
                    }

                    ProgressEnricher.IncrementStep();
                }

                barrier.SignalAndWait(forcedCancellation.Token);
            });
        }
            
        // Start all workers.
        for (var i = 0; i < workerThreads.Length; i++)
            workerThreads[i].Start(i + 1);

        // Wait until all complete.
        barrier.SignalAndWait(forcedCancellation.Token);

        // Finalize.
        ProgressEnricher.CurrentStep = ProgressEnricher.TotalSteps;
        if (successCount < files.Count)
            Log.Warning("Analyzed {SuccessCount}/{Count} files successfully", successCount, files.Count);
        else
            Log.Information("Analyzed all files successfully");
    }
        
    /// <summary>
    /// Performs the analysis on an observation of a sample.
    /// </summary>
    /// <param name="logger">The logger of the worker thread to sue.</param>
    /// <param name="file">The file that was processed.</param>
    /// <param name="executionResult">The result of the observation.</param>
    /// <param name="dumpMarking">
    /// Indicates whether after every analysis the markings of the behavior nets should be dumped.
    /// </param>
    /// <param name="analysisTimeout">
    /// Specifies the maximum amount of time an analysis of an event stream can take before it gets cut off.
    /// </param>
    private static void DoAnalysisAndReportResults(
        ILogger logger,
        string file,
        ExecutionResult executionResult,
        bool dumpMarking,
        TimeSpan analysisTimeout)
    {
        logger.Information("Analyzing logs...");
            
        // If event stream is empty, there might be something wrong, so report it.
        if (executionResult.EventStream.Events.Count == 0)
        {
            logger.Warning("Execution trace is empty");
            _resultsWriter.ReportEmptyAnalysisResult(file, executionResult.Hash);
            return;
        }

        // Analyze event stream.
        using var tokenSource = new CancellationTokenSource(analysisTimeout);
        var analysisResult = Analyzer.Analyze(executionResult.EventStream, tokenSource.Token);
        if (tokenSource.IsCancellationRequested)
            logger.Warning("Analysis time exceeded maximum time limit");

        // Report results to the output.
        _resultsWriter.ReportAnalysisResult(file, executionResult.Hash, analysisResult);
        if (dumpMarking)
            ReportMarkings(logger, analysisResult);

        // If nothing detected, state such.
        if (!analysisResult.HasDetectedBehavior)
        {
            logger.Information("Detecting nothing in {Sample} ({Hash})",
                file,
                executionResult.Hash);
            return;
        }

        // Otherwise, list all detected behaviors.
        foreach (var behavior in analysisResult.DetectedBehaviors)
        {
            logger.Information("Detected {Name} in {Sample} ({Hash})", 
                behavior.Net.Name, 
                file, 
                executionResult.Hash);

            var finalTokens = behavior.Net
                .GetAcceptingPlaces()
                .SelectMany(p => behavior.GetTokens(p))
                .ToArray();

            foreach (var token in finalTokens.Take(5))
                logger.Debug("{Token}", token);

            if (finalTokens.Length > 5)
                logger.Debug($"... and {finalTokens.Length - 5} more ...");
        }
    }

    /// <summary>
    /// Reports the markings of all behavior nets in the repository.
    /// </summary>
    /// <param name="logger">The logger to report the markings to.</param>
    /// <param name="result">The result containing the markings.</param>
    private static void ReportMarkings(ILogger logger, AnalysisResult result)
    {
        foreach (var marking in result.Markings)
        {
            foreach (var place in marking.Net.Places)
            {
                string[] tokens = marking.GetTokens(place).Select(t => t.ToString()).ToArray();
                logger.Information("{Graph}@{Name}: {Tokens}", marking.Net.Name, place.Name, tokens);
            }
        }
    }

    /// <summary>
    /// Configures the command-line application with general parameters.
    /// </summary>
    /// <param name="logFile">
    /// Specifies a path to the file to write logs to, or <c>null</c> if no log file is to be
    /// produced.
    /// </param>
    /// <param name="verbose">Indicates whether the output should include verbose output.</param>
    /// <param name="driverName">The driver to use for performing the examinations.</param>
    /// <param name="resultsPath">
    /// The path to the file that aggregates all observation and analysis results, or
    /// <c>null</c> if no file is to be produced.
    /// </param>
    private static void ConfigureTool(string? logFile, bool verbose, string driverName, string? resultsPath)
    {
        ConfigureLogger(logFile, verbose);
        CompileBehaviors();
        _driver = DriverRepository.GetDriver(driverName);

        if (!string.IsNullOrEmpty(resultsPath))
            _resultsWriter = new FileResultsWriter(resultsPath);
    }

    /// <summary>
    /// Configures the main logger instance of the application.
    /// </summary>
    /// <param name="logFile">
    /// Specifies a path to the file to write logs to, or <c>null</c> if no log file is to be produced.
    /// </param>
    /// <param name="verbose">Indicates whether the output should include verbose output.</param>
    private static void ConfigureLogger(string? logFile, bool verbose)
    {
        var config = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .Enrich.With(ProgressEnricher)
            .WriteTo.Console(verbose ? LogEventLevel.Verbose : LogEventLevel.Information, LogFormat);

        if (logFile is not null)
            config = config.WriteTo.File(logFile, verbose ? LogEventLevel.Verbose : LogEventLevel.Debug, LogFormat);

        Log.Logger = config.CreateLogger();
    }

    /// <summary>
    /// Compiles all behavior nets in the <c>Behaviors</c> directory, and puts them in the analyzer's behavior graph
    /// repository.
    /// </summary>
    private static void CompileBehaviors()
    {
        Log.Information("Compiling behavior analyzer...");
        string[] behaviorNetFiles = Directory.EnumerateFiles(Path.Combine(ProgramDirectory, "Behaviors"), "*.behavior").ToArray();

        foreach (string file in behaviorNetFiles)
        {
            Log.Debug("Compiling {File}...", file);
            try
            {
                var behavior = BehaviorNetFactory.FromFile(file);
                Analyzer.Behaviors.Add(behavior);
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                Log.Error("{Error}", ex.Message);
            }
        }
    }

}