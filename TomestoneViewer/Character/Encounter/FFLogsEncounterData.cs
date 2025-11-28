using System;
using System.Collections.Generic;
using System.Linq;

namespace TomestoneViewer.Character.Encounter;

public record FFLogsEncounterData(IReadOnlyDictionary<JobId, FFLogsEncounterData.CClearCount> ClearsPerJob)
{
    public FFLogsEncounterData.CClearCount AllClears => CClearCount.Compile(this.ClearsPerJob.Values);


    public FFLogsEncounterData.CClearCount Clears(JobId.RoleId role)
    {
        return CClearCount.Compile(
            this.ClearsPerJob
            .Where(pair => pair.Key.GetRoleId() == role)
            .Select(pair => pair.Value)
        );
    }

    internal static FFLogsEncounterData Compile(IReadOnlyList<FFLogsEncounterData> encounterProgress)
    {
        return new(encounterProgress.SelectMany(data => data.ClearsPerJob)
                         .ToLookup(pair => pair.Key, pair => pair.Value)
                         .ToDictionary(group => group.Key, group => CClearCount.Compile(group.ToList())));
    }

    public record CClearCount(uint ThisExpansion, uint PreviousExpansions, DateTime LastClear)
    {
        public uint Total => this.ThisExpansion + this.PreviousExpansions;


        public CClearCount(uint clears, DateTime lastClear, bool isPreviousExpansion)
            : this(isPreviousExpansion ? 0 : clears, isPreviousExpansion ? clears : 0, lastClear)
        {
        }


        internal static CClearCount Compile(IEnumerable<CClearCount> clears)
        {
            uint thisExp = 0;
            uint prevExp = 0;
            var lastClear = DateTime.MinValue;
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
