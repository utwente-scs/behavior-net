namespace BehaviorNets.Drivers.Drakvuf.Reporting;

public abstract class DrakvufEvent
{
    public string TimeStamp
    {
        get;
        set;
    }

    public uint PID
    {
        get;
        set;
    }

    public uint PPID
    {
        get;
        set;
    }

    public uint TID
    {
        get;
        set;
    }

    public string Method
    {
        get;
        set;
    }

    // Ignoring these properties since they are not used, which means we can save a bunch of allocations.

    // public string Plugin
    // {
    //     get;
    //     set;
    // }

    // public string UserName
    // {
    //     get;
    //     set;
    // }
    //
    // public int UserId
    // {
    //     get;
    //     set;
    // }

    // public string ProcessName
    // {
    //     get;
    //     set;
    // }

    // public string EventUID
    // {
    //     get;
    //     set;
    // }
}