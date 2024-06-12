using System;

namespace TomestoneViewer.Character.Encounter.Data.Tomestone;

public record EncounterClear(DateTime? DateTime, string? CompletionWeek)
{
    public bool HasInfo => this.DateTime.HasValue;

    public string? CalculateCompletionWeek(DateTime? releaseDate)
    {
        if (releaseDate == null || this.DateTime == null)
        {
            return null;
        }

        var diff = (this.DateTime - releaseDate).Value;
        var weeks = (int)(diff.TotalDays / 7) + 1;
        return $"Week {weeks}";
    }
}
