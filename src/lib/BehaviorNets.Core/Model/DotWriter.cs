using System.Collections.Generic;
using System.IO;

namespace BehaviorNets.Model;

/// <summary>
/// Provides a mechanism for serializing nets into the GraphViz DOT language.
/// </summary>
public class DotWriter
{
    private readonly TextWriter _output;
    private readonly Dictionary<INode, string> _nodeIds = new();
    private int _placeCounter;
    private int _transitionCounter;

    /// <summary>
    /// Creates a new dot writer that writes to the provided text output stream.
    /// </summary>
    /// <param name="output">The output stream.</param>
    public DotWriter(TextWriter output)
    {
        _output = output;
    }

    /// <summary>
    /// Converts a net to the DOT language, and writes the result to the output stream.
    /// </summary>
    /// <param name="net">The net to convert.</param>
    public void Write(BehaviorNet net)
    {
        DetermineNodeIdentifiers(net);
        WriteHeader(net);
        WritePlaces(net);
        WriteTransitions(net);
        WriteArcs(net);
        WriteFooter();
    }

    private void WritePlaces(BehaviorNet net)
    {
        foreach (var place in net.Places)
        {
            WriteIndentation();
            _output.Write(_nodeIds[place]);
            _output.Write(" [label=");
            WriteIdentifier(place.Name);
            _output.Write(", shape=oval");
            if (place.IsAccepting)
                _output.Write(", peripheries=2");
            _output.WriteLine("]");
        }
    }

    private void WriteTransitions(BehaviorNet net)
    {
        foreach (var transition in net.Transitions)
        {
            WriteIndentation();
            _output.Write(_nodeIds[transition]);
            _output.Write(" [label=");
            WriteIdentifier(transition.TransitionFunction?.ToString() ?? transition.Name);
            _output.WriteLine(", shape=rectangle]");
        }
    }

    private void WriteArcs(BehaviorNet net)
    {
        foreach (var transition in net.Transitions)
        {
            foreach (var inputPlace in transition.InputPlaces)
                WriteArc(inputPlace, transition);
            foreach (var outputPlace in transition.OutputPlaces)
                WriteArc(transition, outputPlace);
        }
    }

    private void WriteArc(INode from, INode to)
    {
        WriteIndentation();
        _output.Write(_nodeIds[@from]);
        _output.Write(" -> ");
        _output.WriteLine(_nodeIds[to]);
    }

    private void DetermineNodeIdentifiers(BehaviorNet net)
    {
        foreach (var place in net.Places)
            _nodeIds.Add(place, $"p{_placeCounter++}");
        foreach (var transition in net.Transitions)
            _nodeIds.Add(transition, $"t{_transitionCounter++}");
    }

    private void WriteHeader(BehaviorNet net)
    {
        _output.Write("digraph ");
        if (!string.IsNullOrEmpty(net.Name))
        {
            WriteIdentifier(net.Name);
            _output.Write(' ');
        }

        _output.WriteLine("{");

        WriteIndentation();
        _output.WriteLine("node [fontname=\"Courier New\"]");
    }

    private void WriteFooter() => _output.WriteLine("}");

    private void WriteIndentation() => _output.Write("    ");

    private void WriteIdentifier(string identifier)
    {
        _output.Write('"');
        foreach (char c in identifier)
        {
            if (c == '\n')
            {
                _output.Write("\\l");
            }
            else
            {
                if (c is '"' or '\r' or '\t' or '\\')
                    _output.Write('\\');
                _output.Write(c);
            }
        }
        _output.Write('"');
    }
}