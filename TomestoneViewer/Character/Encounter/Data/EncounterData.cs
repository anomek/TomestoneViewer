using TomestoneViewer.Character.Client.TomestoneClient;
using TomestoneViewer.Character.Encounter.Data.FFLogs;
using TomestoneViewer.Character.Encounter.Data.Tomestone;

namespace TomestoneViewer.Character.Encounter.Data;

public class EncounterData
{
    public SourceEncounterData<TomestoneData> Tomestone { get; } = new();

    public SourceEncounterData<FFLogsData> FFLogs { get; } = new();
}
