using System.IO;
using System.Linq;
using System.Text.Json;

namespace BehaviorNets.CLI.Analytics;

/// <summary>
/// Implements a <see cref="IResultsWriter"/> that writes all reports to a single file, where each report is encoded
/// on a single line of JSON.
/// </summary>
public class FileResultsWriter : IResultsWriter
{
    private readonly StreamWriter _writer;

    /// <summary>
    /// Creates a new instance of the <see cref="FileResultsWriter"/> class.
    /// </summary>
    /// <param name="outputPath">The file to write to.</param>
    public FileResultsWriter(string outputPath)
    {
        _writer = new StreamWriter(outputPath, true);
    }

    /// <inheritdoc />
    public void ReportAnalysisResult(string file, string hash, AnalysisResult result)
    {
        var entry = new ResultsEntry(file, hash, result.DetectedBehaviors.Select(b => b.Net.Name).ToArray()!);
        WriteEntryToFile(entry);
    }

    public void ReportEmptyAnalysisResult(string file, string hash)
    {
        var entry = new ResultsEntry(file, hash, true);
        WriteEntryToFile(entry);
    }

    private void WriteEntryToFile(ResultsEntry entry)
    {
        string serialized = JsonSerializer.Serialize(entry);

        lock (_writer)
        {
            _writer.WriteLine(serialized);
            _writer.Flush();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _writer.Dispose();
    }
}