using System.Collections.Generic;
using BehaviorNets.Model.Evaluation;

namespace BehaviorNets
{
    /// <summary>
    /// A report containing the results of an analysis of an event stream.
    /// </summary>
    public class AnalysisResult
    {
        /// <summary>
        /// Determines whether at least one behavior was detected in the sample.
        /// </summary>
        public bool HasDetectedBehavior => DetectedBehaviors.Count > 0;

        /// <summary>
        /// Gets all behavior net markings that were produced during the analysis.
        /// </summary>
        public ICollection<BehaviorNetMarking> Markings
        {
            get;
        } = new List<BehaviorNetMarking>();

        /// <summary>
        /// Gets the behavior net markings that had tokens in accepting states.
        /// </summary>
        public ICollection<BehaviorNetMarking> DetectedBehaviors
        {
            get;
        } = new List<BehaviorNetMarking>();
    }
}
