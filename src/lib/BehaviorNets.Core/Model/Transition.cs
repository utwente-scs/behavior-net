using BehaviorNets.Model.Collections;

namespace BehaviorNets.Model;

/// <summary>
/// Represents a single transition in a behavior net.
/// </summary>
public class Transition : INode
{
    /// <summary>
    /// Creates a new named transition.
    /// </summary>
    /// <param name="name">The name of the transition.</param>
    public Transition(string name)
    {
        Name = name;
        InputPlaces = new AdjacentPlaceCollection(this, true);
        OutputPlaces = new AdjacentPlaceCollection(this, false);
    }

    /// <summary>
    /// Gets the name of the transition.
    /// </summary>
    public string Name
    {
        get;
    }

    /// <summary>
    /// Gets a collection of input places that this transition will consume tokens from.
    /// </summary>
    public AdjacentPlaceCollection InputPlaces
    {
        get;
    }

    /// <summary>
    /// Gets a collection of output places that this transition will send newly produced tokens to.
    /// </summary>
    public AdjacentPlaceCollection OutputPlaces
    {
        get;
    }

    /// <summary>
    /// Gets the transition function responsible for processing events and producing new tokens.
    /// </summary>
    public ITransitionFunction? TransitionFunction
    {
        get;
        set;
    }

    /// <summary>
    /// Connects the transition to the provided input place.
    /// </summary>
    /// <param name="place">The input place to connect to.</param>
    /// <returns>The current transition.</returns>
    public Transition WithInput(Place place)
    {
        InputPlaces.Add(place);
        return this;
    }

    /// <summary>
    /// Connects the transition to the provided input places.
    /// </summary>
    /// <param name="places">The input places to connect to.</param>
    /// <returns>The current transition.</returns>
    public Transition WithInputs(params Place[] places)
    {
        foreach (var place in places)
            InputPlaces.Add(place);
        return this;
    }

    /// <summary>
    /// Connects the transition to the provided output place.
    /// </summary>
    /// <param name="place">The output place to connect to.</param>
    /// <returns>The current transition.</returns>
    public Transition WithOutput(Place place)
    {
        OutputPlaces.Add(place);
        return this;
    }

    /// <summary>
    /// Connects the transition to the provided output places.
    /// </summary>
    /// <param name="places">The output places to connect to.</param>
    /// <returns>The current transition.</returns>
    public Transition WithOutputs(params Place[] places)
    {
        foreach (var place in places)
            OutputPlaces.Add(place);
        return this;
    }

    /// <summary>
    /// Assigns a transition function to the transition.
    /// </summary>
    /// <param name="transitionFunction">The function to assign.</param>
    /// <returns>The current transition.</returns>
    public Transition WithFunction(ITransitionFunction transitionFunction)
    {
        TransitionFunction = transitionFunction;
        return this;
    }

    /// <inheritdoc />
    public override string ToString() => Name;
}