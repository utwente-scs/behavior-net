using System;
using System.Runtime.Serialization;

namespace BehaviorNets.Drivers.Drakvuf.Api;

[Serializable]
public class DrakvufApiException : Exception
{
    public DrakvufApiException()
    {
    }

    public DrakvufApiException(string message)
        : base(message)
    {
    }

    public DrakvufApiException(string message, Exception inner)
        : base(message, inner)
    {
    }
}