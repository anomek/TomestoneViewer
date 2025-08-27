using System.Collections.Generic;

namespace TomestoneViewer.Character.Encounter;

public record FFLogsData(IReadOnlyDictionary<JobId, FFLogsData.CClearCount> ClearsPerJob)
{
    public record CClearCount(uint ThisExpansion, uint PreviousExpansion);
}
