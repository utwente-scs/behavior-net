using System.Collections.Generic;
using System.IO;
using System.Linq;
using BehaviorNets.Model.Evaluation;

namespace BehaviorNets.Model;

/// <summary>
/// Provides an implementation for a behavior net using a petri net.
/// </summary>
/// <remarks>
/// This implementation only defines the structure of the net. For actual evaluation, use
/// the <see cref="BehaviorNetEvaluator"/> class.
/// </remarks>
public class BehaviorNet
{
    /// <summary>
    /// Creates a new empty, unnamed behavior net.
    /// </summary>
    public BehaviorNet()
    {
    }

    /// <summary>
    /// Creates a new empty, named behavior net.
    /// </summary>
    /// <param name="name">The name of the net.</param>
    public BehaviorNet(string? name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets the name of the net.
    /// </summary>
    public string? Name
    {
        get;
    }

    /// <summary>
    /// Gets a collection of places that this net defines.
    /// </summary>
    public IList<Place> Places
    {
        get;
    } = new List<Place>();

    /// <summary>
    /// Gets a collection of transitions that this net defines.
    /// </summary>
    public IList<Transition> Transitions
    {
        get;
    } = new List<Transition>();

    /// <summary>
    /// Obtains a collection of all places in the net that are marked as accepting.
    /// </summary>
    /// <returns>The accepting places.</returns>
    public IEnumerable<Place> GetAcceptingPlaces() => Places.Where(p => p.IsAccepting);

    /// <summary>
    /// Adds a named place to the net.
    /// </summary>
    /// <param name="name">The name of the place.</param>
    /// <returns>The newly added place.</returns>
    public Place AddPlace(string name)
    {
        var place = new Place(name);
        Places.Add(place);
        return place;
    }

    /// <summary>
    /// Adds two named places to the net.
    /// </summary>
    /// <param name="name1">The name of the first place.</param>
    /// <param name="name2">The name of the second place.</param>
    /// <returns>The newly added places.</returns>
    public (Place, Place) AddPlaces(string name1, string name2)
    {
        var result = AddPlaces(new[] {name1, name2});
        return (result[0], result[1]);
    }

    /// <summary>
    /// Adds three named places to the net.
    /// </summary>
    /// <param name="name1">The name of the first place.</param>
    /// <param name="name2">The name of the second place.</param>
    /// <param name="name3">The name of the third place.</param>
    /// <returns>The newly added places.</returns>
    public (Place, Place, Place) AddPlaces(string name1, string name2, string name3)
    {
        var result = AddPlaces(new[] {name1, name2, name3});
        return (result[0], result[1], result[2]);
    }

    /// <summary>
    /// Adds four named places to the net.
    /// </summary>
    /// <param name="name1">The name of the first place.</param>
    /// <param name="name2">The name of the second place.</param>
    /// <param name="name3">The name of the third place.</param>
    /// <param name="name4">The name of the fourth place.</param>
    /// <returns>The newly added places.</returns>
    public (Place, Place, Place, Place) AddPlaces(string name1, string name2, string name3, string name4)
    {
        var result = AddPlaces(new[] {name1, name2, name3, name4});
        return (result[0], result[1], result[2], result[3]);
    }

    /// <summary>
    /// Adds a list of named places to the net.
    /// </summary>
    /// <param name="names">The names of the new places.</param>
    /// <returns>The newly added places.</returns>
    public Place[] AddPlaces(params string[] names)
    {
        var places = new Place[names.Length];

        for (int i = 0; i < places.Length; i++)
        {
            var place = new Place(names[i]);
            Places.Add(place);
            places[i] = place;
        }

        return places;
    }

    /// <summary>
    /// Adds a named transition to the net.
    /// </summary>
    /// <param name="name">The name of the new transition</param>
    /// <returns>The newly added transition.</returns>
    public Transition AddTransition(string name)
    {
        var transition = new Transition(name);
        Transitions.Add(transition);
        return transition;
    }

    /// <summary>
    /// Adds a list of named transitions to the net.
    /// </summary>
    /// <param name="names">The names of the new transitions.</param>
    /// <returns>The newly added transitions.</returns>
    public Transition[] AddTransitions(params string[] names)
    {
        var transitions = new Transition[names.Length];

        for (int i = 0; i < transitions.Length; i++)
        {
            var transition = new Transition(names[i]);
            Transitions.Add(transition);
            transitions[i] = transition;
        }

        return transitions;
    }

    /// <summary>
    /// Serializes the net to the GraphViz DOT language format.
    /// </summary>
    /// <returns>The serialized net.</returns>
    public string ToGraphViz()
    {
        var writer = new StringWriter();
        ToGraphViz(writer);
        return writer.ToString();
    }

    /// <summary>
    /// Serializes the net to the GraphViz DOT language format.
    /// </summary>
    /// <param name="output">The output stream to write the net to.</param>
    public void ToGraphViz(TextWriter output)
    {
        var writer = new DotWriter(output);
        writer.Write(this);
    }
}