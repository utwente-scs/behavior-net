using BehaviorNets.Data;
using BehaviorNets.Model.Evaluation;

namespace BehaviorNets.Model;

/// <summary>
/// Represents a transition function that can be used to consume and produce new tokens in a behavior net.
/// </summary>
public interface ITransitionFunction
{
    /// <summary>
    /// Evaluates the transition function for the provided event and input token.
    /// </summary>
    /// <param name="event">The event to process.</param>
    /// <param name="token">The input and output token.</param>
    /// <returns><c>true</c> if the transition function produced a valid token, <c>false</c> otherwise.</returns>
    /// <remarks>
    /// This function may modify the contents of <paramref name="token"/> even if it returns <c>false</c>.
    /// </remarks>
    bool Evaluate(ExecutionEvent @event, ref Token token);
}