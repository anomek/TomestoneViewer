using System;
using System.Collections.Generic;
using System.Linq;

namespace TomestoneViewer.Character.Encounter.Data.Tomestone;

public record ProgPoint(IReadOnlyList<ProgPoint.Lockout> Lockouts)
{
    private static readonly int LockoutsLimit = 5;
    private static readonly int PhaseThreshold = 1;
    private static readonly int PercentThreshold = 25;

    public IReadOnlyList<ProgPoint.Lockout> LockoutsBrief => this.Lockouts.Take(LockoutsLimit).ToList();

    public IReadOnlyList<JobId> Jobs
    {
        get
        {
            var last = this.LockoutsBrief.Count == 0 ? null : this.LockoutsBrief[this.LockoutsBrief.Count - 1].Percent;
            if (last == null)
            {
                return [];
            }

            return this.Lockouts
                .TakeWhile(lockout => lockout.Percent.WithinThreashold(last, PhaseThreshold, PercentThreshold))
                .Select(lockout => lockout.Job)
                .OfType<JobId>()
                .Distinct()
                .ToList();
        }
    }

    public override string ToString()
    {
        return this.Lockouts[0].Percent.ToString();
    }

    public record Lockout(Percent Percent, DateOnly? Timestamp, JobId? Job)
    {
    }

    public record Percent
    {
        public override string ToString()
        {
            var prefix = this.Phase != null ? $"P{this.Phase}" : string.Empty;
            var number = this.Number.HasValue ? $"{this.Number.Value}%" : string.Empty;
            var space = prefix != string.Empty && number != string.Empty ? " " : string.Empty;
            return $"{prefix}{space}{number}";
        }

        public int? Phase { get; private init; }

        public int? Number { get; private init; }

        internal bool WithinThreashold(Percent other, int phaseThreashold, int percentThreshold)
        {
            if (this.Phase != null && other.Phase != null)
            {
                return this.Phase + phaseThreashold >= other.Phase;
            }
            else if (this.Number != null && other.Number != null)
            {
                return this.Number + percentThreshold >= other.Number;
            }
            else
            {
                return false;
            }
        }

        public static Percent From(string value)
        {
            var hasNumber = int.TryParse(value.Split('%')[0].Split('.')[0], out var number);
            var phases = value.Split('P');
            return new()
            {
                Phase = phases.Length > 1 ? int.Parse(phases[1]) : null,
                Number = hasNumber ? number : null,
            };
        }
    }
}
