using System;
using System.Collections.Generic;

namespace TomestoneViewer.Character.Encounter;

public record ProgPoint(IReadOnlyList<ProgPoint.Lockout> Lockouts)
{
    public override string ToString()
    {
        return this.Lockouts[0].Percent.ToString();
    }

    public record Lockout(Percent Percent, DateOnly Timestamp)
    {
    }

    public record Percent
    {
        public override string ToString()
        {
            var prefix = this.Phase != null ? $"P{this.Phase} " : string.Empty;
            return $"{prefix}{this.Number}%";
        }

        public string? Phase { get; private init; }

        public int Number { get; private init; }

        public static Percent From(string value)
        {
            var number = int.Parse(value.Split('.')[0]);
            var phases = value.Split('P');
            return new()
            {
                Phase = phases.Length > 1 ? phases[1] : null,
                Number = number,
            };
        }
    }
}
