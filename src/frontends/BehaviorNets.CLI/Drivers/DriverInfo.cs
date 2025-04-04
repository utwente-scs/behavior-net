namespace BehaviorNets.CLI.Drivers;

/// <summary>
/// Provides metadata about an execution driver plugin.
/// </summary>
public struct DriverInfo
{
    /// <summary>
    /// Gets or sets the name of the driver.
    /// </summary>
    public string Name
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the path to the library implementing the driver.
    /// </summary>
    public string Path
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the type within the library to instantiate. 
    /// </summary>
    public string Type
    {
        get;
        set;
    }
}