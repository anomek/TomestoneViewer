using System.Numerics;

using TomestoneViewer.Character;
using TomestoneViewer.Character.Encounter;
using TomestoneViewer.GUI.Widgets;

namespace TomestoneViewer.GUI.Main;

internal class EncounterStatusView : IWidget
{
    private readonly ClearCountWidget clearCountWidget = new()
    {
        IncludeLastClear = true,
    };

    private readonly TomestoneProgWidget progWidget = new();

    public EncounterData? EncounterData { get; set; }

    public CharData? CharData { get; set; }

    public bool Total { get; set; }

    public float? BaseLine { get; set; } = null;

    public float YOffset { get; set; }

    public Vector2 Draw()
    {
        if (this.CharData == null || this.EncounterData == null)
        {
            return default;
        }

        if (this.EncounterData.FFLogs.Data != null && this.EncounterData.FFLogs.Data.RecordedClear && Service.Configuration.FFLogsEnabled)
        {
            var data = this.EncounterData.FFLogs.Data;
            this.clearCountWidget.Clears = this.Total ? data.AllClears : data.Clears(this.CharData.JobId.GetRoleId());
            this.clearCountWidget.BaseLine = this.BaseLine;
            this.clearCountWidget.YOffset = this.YOffset;
            return this.clearCountWidget.Draw();
        }
        else if (this.Total)
        {
            this.progWidget.GenericTomestoneError = this.CharData.GenericTomestoneError;
            this.progWidget.Data = this.EncounterData.Tomestone;
            this.progWidget.BaseLine = this.BaseLine;
            this.progWidget.YOffset = this.YOffset;
            return this.progWidget.Draw();
        }

        return default;
    }

    public float GetMinWidth()
    {
        return float.Max(this.clearCountWidget.GetMinWidth(), this.progWidget.GetMinWidth());
    }
}
