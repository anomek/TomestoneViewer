using System;
using System.Collections.Generic;
using System.Linq;

namespace TomestoneViewer.Character.Encounter;

public record FFLogsEncounterData(IReadOnlyDictionary<JobId, FFLogsEncounterData.CClearCount> ClearsPerJob)
{
    internal static FFLogsEncounterData Compile(IReadOnlyList<FFLogsEncounterData> encounterProgress)
    {
        return new(encounterProgress.SelectMany(data => data.ClearsPerJob)
                         .ToLookup(pair => pair.Key, pair => pair.Value)
                         .ToDictionary(group => group.Key, group => CClearCount.Compile(group.ToList())));
    }

    public record CClearCount(uint ThisExpansion, uint PreviousExpansions, DateOnly LastClear)
    {
        public uint Total => this.ThisExpansion + this.PreviousExpansions;
        internal static CClearCount Compile(IReadOnlyList<CClearCount> clears)
        {
            uint thisExp = 0;
            uint prevExp = 0;
            DateOnly lastClear = DateOnly.MinValue;
            foreach (var clear in clears)
            {
                thisExp += clear.ThisExpansion;
                prevExp += clear.PreviousExpansions;
                lastClear = clear.LastClear > lastClear ? clear.LastClear : lastClear;
            }

            return new(thisExp, prevExp, lastClear);
        }
    }
}
