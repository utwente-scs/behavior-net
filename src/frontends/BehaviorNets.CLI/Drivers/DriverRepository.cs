using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Loader;
using System.Text.Json;
using BehaviorNets.Data;
using BehaviorNets.Json;

namespace BehaviorNets.CLI.Drivers;

/// <summary>
/// Provides a collection of execution driver plugins that can be used for examining samples and/or parsing event
/// streams.
/// </summary>
public class DriverRepository
{
    private static readonly JsonSerializerOptions SerializerOptions =  new()
    {
        PropertyNamingPolicy = SnakeCasePolicy.Instance
    };

    private readonly string _driversDirectory;
    private readonly Dictionary<string, DriverInfo> _driverInfos = new();
    private readonly Dictionary<string, IExecutionDriver> _drivers = new();

    /// <summary>
    /// Creates a new driver plugin repository.
    /// </summary>
    /// <param name="driversDirectory">The directory to search drivers in.</param>
    /// <param name="configPath">The path to the driver configuration.</param>
    public DriverRepository(string driversDirectory, string configPath)
    {
        _driversDirectory = driversDirectory;

        var config = JsonSerializer.Deserialize<DriverConfiguration>(
            File.ReadAllText(configPath),
            SerializerOptions);

        foreach (var info in config.Drivers)
            _driverInfos.Add(info.Name, info);

#if DEBUG
            _drivers.Add("dummy", new DummyExecutionDriver());
#endif
    }

    /// <summary>
    /// Gets the driver by its identifier.
    /// </summary>
    /// <param name="identifier">The identifier of the driver.</param>
    /// <returns>The driver object.</returns>
    public IExecutionDriver GetDriver(string identifier)
    {
        if (!_drivers.TryGetValue(identifier, out var driver))
        {
            if (!_driverInfos.TryGetValue(identifier, out var info))
                throw new ArgumentException($"No driver with identifier '{identifier}' installed.");

            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(_driversDirectory, info.Path));
            var type = assembly.GetType(info.Type, true)!;
            driver = (IExecutionDriver) Activator.CreateInstance(type)!;
            _drivers.Add(identifier, driver);
        }

        return driver;
    }
}