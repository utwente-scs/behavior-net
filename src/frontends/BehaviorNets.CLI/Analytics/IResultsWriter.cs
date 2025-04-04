using System;

namespace BehaviorNets.CLI.Analytics;

/// <summary>
/// Provides members for reporting analysis reports.
/// </summary>
public interface IResultsWriter : IDisposable
{
    /// <summary>
    /// Reports a single analysis report.
    /// </summary>
    /// <param name="file">The sample that was analyzed.</param>
    /// <param name="hash">The hash of the sample.</param>
    /// <param name="result">The analysis report.</param>
    void ReportAnalysisResult(string file, string hash, AnalysisResult result);
        
    /// <summary>
    /// Reports a single analysis report.
    /// </summary>
    /// <param name="file">The sample that was analyzed.</param>
    /// <param name="hash">The hash of the sample.</param>
    void ReportEmptyAnalysisResult(string file, string hash);
}