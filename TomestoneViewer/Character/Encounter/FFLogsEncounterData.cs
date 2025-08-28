using System;
using System.Collections.Generic;

namespace TomestoneViewer.Character.Encounter;

public record FFLogsEncounterData(IReadOnlyDictionary<JobId, FFLogsEncounterData.CClearCount> ClearsPerJob)
{
    public record CClearCount(uint ThisExpansion, uint PreviousExpansion, DateOnly LastClear);
}
