using Sandbox;

namespace ShrimpleProfiler.ProfilerEvents;

public class RemovedProfileEventAttribute : EventAttribute
{
    public const string Name = "profiler.removed";

    public RemovedProfileEventAttribute() : base(Name)
    {
    }
}
