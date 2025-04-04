using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace BehaviorNets.Model.Collections;

/// <summary>
/// Represents a collection of transitions that are adjacent to a place in a behavior net.
/// </summary>
[DebuggerDisplay("Count = {" + nameof(Count) + "}")]
public class AdjacentTransitionCollection : ICollection<Transition>
{
    private readonly HashSet<Transition> _transitions = new();
    private readonly Place _owner;
    private readonly bool _isInput;

    internal AdjacentTransitionCollection(Place owner, bool isInput)
    {
        _owner = owner;
        _isInput = isInput;
    }

    /// <inheritdoc />
    public int Count => _transitions.Count;

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <summary>
    /// Adds a transition to the collection.
    /// </summary>
    /// <param name="item">The transition to add.</param>
    /// <returns><c>true</c> if the transition was added, <c>false</c> if the transition was already added.</returns>
    public bool Add(Transition item)
    {
        if (_transitions.Add(item))
        {
            GetPlacesList(item).Add(_owner);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    void ICollection<Transition>.Add(Transition item) => Add(item);

    /// <inheritdoc />
    public void Clear()
    {
        foreach (var transition in _transitions)
            Remove(transition);
    }

    /// <inheritdoc />
    public bool Contains(Transition item) => _transitions.Contains(item);

    /// <inheritdoc />
    public void CopyTo(Transition[] array, int arrayIndex) => _transitions.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public bool Remove(Transition item)
    {
        if (_transitions.Remove(item))
        {
            GetPlacesList(item).Remove(_owner);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public IEnumerator<Transition> GetEnumerator() => _transitions.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _transitions).GetEnumerator();

    private ICollection<Place> GetPlacesList(Transition transition) => _isInput
        ? transition.OutputPlaces
        : transition.InputPlaces;
}