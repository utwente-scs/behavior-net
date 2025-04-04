using BehaviorNets.Model.Collections;

namespace BehaviorNets.Model;

/// <summary>
/// Represents a single place in a behavior net.
/// </summary>
public class Place : INode
{
    /// <summary>
    /// Creates a new named place.
    /// </summary>
    /// <param name="name">The name of the place.</param>
    public Place(string name)
    {
        Name = name;
        InputTransitions = new AdjacentTransitionCollection(this, true);
        OutputTransitions = new AdjacentTransitionCollection(this, false);
    }

    /// <summary>
    /// Gets the name of the place.
    /// </summary>
    public string Name
    {
        get;
    }

    /// <summary>
    /// Gets a collection of input transitions that might provide this place with tokens.
    /// </summary>
    public AdjacentTransitionCollection InputTransitions
    {
        get;
    }

    /// <summary>
    /// Gets a collection of input transitions that might pull tokens from this place.
    /// </summary>
    public AdjacentTransitionCollection OutputTransitions
    {
        get;
    }

    /// <summary>
    /// Gets a value indicating whether the place is an accepting place or not.
    /// </summary>
    public bool IsAccepting
    {
        get;
        set;
    }

    /// <inheritdoc />
    public override string ToString() => IsAccepting
        ? $"{Name} (Accepting)"
        : Name;
}