using Sandbox;

namespace ShrimpleProfiler.ProfilerEvents;

public class NewProfileEventAttribute : EventAttribute
{
    public const string Name = "profiler.new";

    public NewProfileEventAttribute() : base(Name)
    {
    }
}
