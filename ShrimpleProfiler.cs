using Sandbox;
using ShrimpleProfiler.ProfilerEvents;
using System.Collections.Generic;
using System.Linq;

namespace ShrimpleProfiler;

public partial class ShrimpleProfiler
{
    public const double InactiveProfileRemoveTime = 10;

    public static ShrimpleProfiler The
    {
        get
        {
            _the ??= new ShrimpleProfiler();

            return _the;
        }
    }

    private static ShrimpleProfiler _the;

    private sealed class ProfileMeasures
    {
        public Profile Profile { get; set; }
        public Queue<(double Start, double Elapsed)> Measures = new();
        public Queue<double> Histogram = new(); // TODO: perhaps use a looped buffer?
        public RealTimeSince LastCheck = 0;

        private double Carry = 0;

        public ProfileMeasures(Profile profile)
        {
            Profile = profile;

            for (var i = 0; i < profile.Resolution; i++)
                Histogram.Enqueue(0);
        }

        public void AddMeasure(double Start, double Elapsed)
        {
            Measures.Enqueue((Start, Elapsed));
        }

        /// <summary>
        /// Form a histogram
        /// </summary>
        /// <param name="histogramFormTime"></param>
        /// <returns>true if there is a new column, otherwise false</returns>
        public bool FormHistogram(double histogramFormTime)
        {
            if (LastCheck < Profile.Interval)
                return false;

            bool anyNewData = Measures.Count > 0;

            double time = LastCheck.Absolute;

            do
            {
                double currentColumn = Carry;
                double targetTime = time + Profile.Interval;

                while (currentColumn < Profile.Interval && Measures.Count > 0)
                {
                    var (Start, Elapsed) = Measures.Peek();
                    if (Start > targetTime)
                    {
                        time = targetTime;
                        break;
                    }

                    Measures.Dequeue();
                    time = Start;
                    currentColumn += Elapsed;
                }

                if (currentColumn > Profile.Interval)
                {
                    Carry = currentColumn - Profile.Interval;
                    currentColumn = Profile.Interval;
                }

                Histogram.Enqueue(currentColumn);
                Histogram.Dequeue();
            } while (time < histogramFormTime && Measures.Count > 0);

            if (anyNewData)
                LastCheck = 0;
            return anyNewData;
        }
    }

    private readonly Dictionary<Profile, ProfileMeasures> _profileMeasures = new();

    public ShrimpleProfiler()
    {
        Event.Register(this);

        // TODO: for some reason this root panel gets below everything
        /*if (Game.IsClient)
            _ = new ProfilerPanel();*/
    }

    public void AddMeasure(Profile profile, double start, double elapsed)
    {
        if (!_profileMeasures.ContainsKey(profile))
            _profileMeasures[profile] = new(profile);

        _profileMeasures[profile].AddMeasure(start, elapsed);
    }

    public bool HasActiveProfiles() => _profileMeasures.Count > 0;

    [GameEvent.Tick]
    public void Tick()
    {
        double histogramFormTime = RealTime.GlobalNow;
        var profiles = _profileMeasures.Keys; // Making a copy so that we can modify this dictionary later on
        foreach (var profile in profiles)
        {
            var m = _profileMeasures[profile];
            var hasNewColumn = m.FormHistogram(histogramFormTime);

            if (hasNewColumn)
            {
                Event.Run(NewProfileEventAttribute.Name, profile, m.Histogram.ToList());
            }
            else if (m.LastCheck >= InactiveProfileRemoveTime)
            {
                _profileMeasures.Remove(profile);
                Event.Run(RemovedProfileEventAttribute.Name, profile);
            }
        }
    }
}
