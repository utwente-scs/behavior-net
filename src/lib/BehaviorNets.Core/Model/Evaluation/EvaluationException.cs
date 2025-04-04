using System;
using System.Runtime.Serialization;

namespace BehaviorNets.Model.Evaluation
{
    /// <summary>
    /// Represents an exception that occurs during the evaluation of a behavior net.
    /// </summary>
    [Serializable]
    public class EvaluationException : Exception
    {
        public EvaluationException()
        {
        }

        public EvaluationException(string message)
            : base(message)
        {
        }

        public EvaluationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
