using System.Collections.Immutable;
using System.Linq;

namespace BehaviorNets.Drivers.Drakvuf.Api;

public class TaskCreateParameters
{
    public static readonly ImmutableArray<string> DefaultPlugins = ImmutableArray.Create("apimon", "regmon", "librarymon");

    public string FilePath
    {
        get;
        set;
    }

    public int Timeout
    {
        get;
        set;
    } = 10;

    public string StartCommand
    {
        get;
        set;
    }

    public string[] EnabledPlugins
    {
        get;
        set;
    } = DefaultPlugins.ToArray();
}