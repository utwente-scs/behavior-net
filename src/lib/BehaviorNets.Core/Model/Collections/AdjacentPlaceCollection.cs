using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BehaviorNets.Model.Collections;

/// <summary>
/// Represents a collection of places that are adjacent to a transition in a behavior net.
/// </summary>
[DebuggerDisplay("Count = {" + nameof(Count) + "}")]
public class AdjacentPlaceCollection : ICollection<Place>
{
    private readonly HashSet<Place> _places = new();
    private readonly Transition _owner;
    private readonly bool _isInput;

    internal AdjacentPlaceCollection(Transition owner, bool isInput)
    {
        _owner = owner;
        _isInput = isInput;
    }

    /// <inheritdoc />
    public int Count => _places.Count;

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <summary>
    /// Adds a place to the collection.
    /// </summary>
    /// <param name="item">The place to add.</param>
    /// <returns><c>true</c> if the place was added, <c>false</c> if the place was already added.</returns>
    public bool Add(Place item)
    {
        if (_places.Add(item))
        {
            GetTransitionList(item).Add(_owner);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    void ICollection<Place>.Add(Place item) => Add(item);

    /// <inheritdoc />
    public void Clear()
    {
        foreach (var place in _places.ToArray())
            Remove(place);
    }

    /// <inheritdoc />
    public bool Contains(Place item) => _places.Contains(item);

    /// <inheritdoc />
    public void CopyTo(Place[] array, int arrayIndex) => _places.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public bool Remove(Place item)
    {
        if (_places.Remove(item))
        {
            GetTransitionList(item).Remove(_owner);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public IEnumerator<Place> GetEnumerator() => _places.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => _places.GetEnumerator();

    private ICollection<Transition> GetTransitionList(Place place) => _isInput
        ? place.OutputTransitions
        : place.InputTransitions;
}