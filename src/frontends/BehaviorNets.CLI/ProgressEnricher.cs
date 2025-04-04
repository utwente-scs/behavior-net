using System.Threading;
using Serilog.Core;
using Serilog.Events;

namespace BehaviorNets.CLI;

/// <summary>
/// Enriches a log output with progress indicators. 
/// </summary>
public class ProgressEnricher : ILogEventEnricher
{
    private int _currentStep;
    private int _totalSteps;
    private LogEventProperty? _cachedProperty;

    /// <summary>
    /// Gets or sets the total amount of steps in the task to perform.
    /// </summary>
    public int TotalSteps
    {
        get => _totalSteps;
        set
        {
            if (_totalSteps != value)
            {
                _totalSteps = value;
                _cachedProperty = null;
            }
        }
    }

    /// <summary>
    /// Gets or sets the index of the current step to perform.
    /// </summary>
    public int CurrentStep
    {
        get => _currentStep;
        set
        {
            if (_currentStep != value)
            {
                _currentStep = value;
                _cachedProperty = null;
            }
        }
    }

    /// <summary>
    /// Gets the current total progress percentage. 
    /// </summary>
    public double Progress => TotalSteps > 0 
        ? CurrentStep / (double) TotalSteps 
        : 0;

    /// <summary>
    /// Increments the <see cref="CurrentStep"/> property in an atomic way.
    /// </summary>
    public void IncrementStep()
    {
        Interlocked.Increment(ref _currentStep);
        _cachedProperty = null;
    }

    /// <inheritdoc />
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var property = _cachedProperty;
        if (property is null)
        {
            property = propertyFactory.CreateProperty("Progress", Progress * 100);
            _cachedProperty = property;
        }

        logEvent.AddOrUpdateProperty(property);
    }
}