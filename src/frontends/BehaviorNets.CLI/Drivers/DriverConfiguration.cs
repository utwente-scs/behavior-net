using System.Collections.Generic;

namespace BehaviorNets.CLI.Drivers;

/// <summary>
/// Models the execution driver configuration of the application. 
/// </summary>
public struct DriverConfiguration
{
    /// <summary>
    /// Gets or sets a list of registered execution driver plugins. 
    /// </summary>
    public List<DriverInfo> Drivers
    {
        get;
        set;
    }
}