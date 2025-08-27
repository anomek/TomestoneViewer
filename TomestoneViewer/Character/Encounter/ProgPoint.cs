using System;
using System.Collections.Generic;

namespace TomestoneViewer.Character.Encounter;

public record ProgPoint(IReadOnlyList<ProgPoint.Lockout> Lockouts)
{
    public override string ToString()
    {
        return this.Lockouts[0].Percent.ToString();
    }

    public record Lockout(Percent Percent, DateOnly? Timestamp, JobId Job)
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

        public string? Phase { get; private init; }

        public int? Number { get; private init; }

        public static Percent From(string value)
        {
            var hasNumber = int.TryParse(value.Split('%')[0].Split('.')[0], out var number);
            var phases = value.Split('P');
            return new()
            {
                Phase = phases.Length > 1 ? phases[1] : null,
                Number = hasNumber ? number : null,
            };
        }
    }
}
