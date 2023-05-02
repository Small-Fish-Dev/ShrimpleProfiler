using Sandbox;
using System;
using System.Runtime.CompilerServices;

namespace ShrimpleProfiler;

public sealed class Measure : IDisposable
{
    private readonly Profile _profile;

    private RealTimeSince _profileStart;

    public Measure([CallerMemberName] string caller = "Unknown method", int resolution = 5, double interval = 0.05)
    {
        _profile = new Profile(caller, resolution, interval);

        _profileStart = 0;
    }

    /// <summary>
    /// Record the measure to the table of profiles
    /// </summary>
    public void Dispose()
    {
        ShrimpleProfiler.The.AddMeasure(_profile, _profileStart.Absolute, _profileStart);
    }
}
