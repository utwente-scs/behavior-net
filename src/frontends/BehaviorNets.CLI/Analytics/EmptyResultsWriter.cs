namespace BehaviorNets.CLI.Analytics;

/// <summary>
/// Implements an empty results writer that silently discards any report. 
/// </summary>
public class EmptyResultsWriter : IResultsWriter
{
    public static EmptyResultsWriter Instance
    {
        get;
    } = new();

    /// <inheritdoc />
    public void ReportAnalysisResult(string file, string hash, AnalysisResult result)
    {
    }

    public void ReportEmptyAnalysisResult(string file, string hash)
    {
    }

    /// <inheritdoc />
    public void Dispose()
    {
    }
}