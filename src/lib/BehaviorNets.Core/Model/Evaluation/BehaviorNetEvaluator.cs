using System.Collections.Immutable;
using BehaviorNets.Data;

namespace BehaviorNets.Model.Evaluation;

/// <summary>
/// Provides a mechanism for feeding events to a behavior net, and evaluating all the transition functions.
/// </summary>
public class BehaviorNetEvaluator
{
    private readonly ImmutableArray<Place> _acceptingPlaces = ImmutableArray<Place>.Empty;

    /// <summary>
    /// Creates a new evaluator for the provided behavior net.
    /// </summary>
    /// <param name="net">The net to evaluate.</param>
    public BehaviorNetEvaluator(BehaviorNet net)
    {
        Net = net;
        Marking = new BehaviorNetMarking(net);

        foreach (var place in net.Places)
        {
            if (place.IsAccepting)
                _acceptingPlaces = _acceptingPlaces.Add(place);
        }
    }

    /// <summary>
    /// Gets the net that is being evaluated.
    /// </summary>
    public BehaviorNet Net
    {
        get;
    }

    /// <summary>
    /// Gets a value indicating whether any of the accepting places in the net has at least one token.
    /// </summary>
    public bool IsAccepting
    {
        get
        {
            foreach (var place in _acceptingPlaces)
            {
                if (Marking.GetTokens(place).Count > 0)
                    return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Gets the current marking of the behavior net.
    /// </summary>
    public BehaviorNetMarking Marking
    {
        get;
    }

    /// <summary>
    /// Feeds an event to the behavior net, and performs all possible transitions.
    /// </summary>
    /// <param name="event">The event to feed the net.</param>
    public void Step(ExecutionEvent @event)
    {
        for (var i = 0; i < Net.Transitions.Count; i++)
        {
            var transition = Net.Transitions[i];
            // Short circuit optimization: We can avoid calling the expensive IsEnabled function if the
            // API name is not matching the event name.
            if (transition.TransitionFunction is ApiTransitionFunction api && api.ApiName != @event.Name)
                continue;

            // Check if enabled.
            if (!Marking.IsEnabled(transition, out var possibleMerges))
                continue;

            foreach (var possibleMerge in possibleMerges)
            {
                var newToken = possibleMerge;

                if (transition.TransitionFunction is { } predicate && !predicate.Evaluate(@event, ref newToken))
                {
                    // Predicate evaluated to "false". This means the matching stops here for this merged token.
                    continue;
                }

                // Move all tokens to the output places.
                foreach (var outputPlace in transition.OutputPlaces)
                    Marking.AddToken(outputPlace, newToken);
            }
        }
    }

}