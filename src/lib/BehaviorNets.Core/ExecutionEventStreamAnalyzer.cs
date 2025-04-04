using System.Collections.Generic;
using System.Threading;
using BehaviorNets.Data;
using BehaviorNets.Model;
using BehaviorNets.Model.Evaluation;

namespace BehaviorNets
{
    /// <summary>
    /// Provides a mechanism for analysing event streams.
    /// </summary>
    public class ExecutionEventStreamAnalyzer
    {
        /// <summary>
        /// Gets a list of behavior nets to recognize.
        /// </summary>
        public IList<BehaviorNet> Behaviors
        {
            get;
        } = new List<BehaviorNet>();

        /// <summary>
        /// Analyzes the provided event stream for any of the fingerprinted behaviors.
        /// </summary>
        /// <param name="eventStream">The event stream to analyze.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the process.</param>
        /// <returns>An analysis report.</returns>
        public AnalysisResult Analyze(ExecutionEventStream eventStream, CancellationToken cancellationToken)
        {
            var result = new AnalysisResult();

            var evaluators = new List<BehaviorNetEvaluator>(Behaviors.Count);
            foreach (var net in Behaviors)
            {
                var evaluator = new BehaviorNetEvaluator(net);
                evaluators.Add(evaluator);
                result.Markings.Add(evaluator.Marking);
            }

            foreach (var e in eventStream.Events)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                for (int i = 0; i < evaluators.Count; i++)
                {
                    var evaluator = evaluators[i];
                    evaluator.Step(e);
                    if (evaluator.IsAccepting)
                    {
                        result.DetectedBehaviors.Add(evaluator.Marking);
                        evaluators.RemoveAt(i);
                        i--;
                    }
                }
            }

            return result;
        }
    }
}
