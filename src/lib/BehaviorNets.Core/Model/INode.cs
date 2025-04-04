namespace BehaviorNets.Model;

/// <summary>
/// Represents a single node in a behavior net.
/// </summary>
public interface INode
{
    /// <summary>
    /// Gets the name of the node.
    /// </summary>
    string Name
    {
        get;
    }
}