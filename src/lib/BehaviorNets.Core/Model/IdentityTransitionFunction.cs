using BehaviorNets.Data;
using BehaviorNets.Model.Evaluation;

namespace BehaviorNets.Model;

/// <summary>
/// Represents the identity transition function, that always evaluates to true and does not change the input token.
/// </summary>
public class IdentityTransitionFunction : ITransitionFunction
{
    /// <summary>
    /// Gets the singleton instance of the identity transition function.
    /// </summary>
    public static IdentityTransitionFunction Instance
    {
        get;
    } = new();

    private IdentityTransitionFunction()
    {
    }

    /// <inheritdoc />
    public bool Evaluate(ExecutionEvent @event, ref Token token) => true;

    /// <inheritdoc />
    public override string ToString() => "true";
}