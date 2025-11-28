using Dalamud.Game.ClientState.Statuses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TomestoneViewer.Character;
using TomestoneViewer.Character.Encounter;
using TomestoneViewer.GUI.Widgets;

namespace TomestoneViewer.GUI.Main;
internal class EncounterStatusView : IWidget
{
    public EncounterData? EncounterData { get; set; }
    public CharData? CharData { get; set; }

    public Func<bool> ffLogsEnabled { get; set; } = () => true;

    public bool Total { get; set; }

    public float? BaseLine { get; set; } = null;
    public float YOffset { get; set; }

    private ClearCountWidget clearCountWidget = new()
    {
        IncludeLastClear = true,
    };

    private TomestoneProgWidget progWidget = new();
    



    public Vector2 Draw()
    {
        if (CharData == null || EncounterData == null)
        {
            return default;
        }

        if (this.EncounterData.FFLogs.Data != null && ffLogsEnabled())
        {
            var data = this.EncounterData.FFLogs.Data;
            clearCountWidget.Clears = this.Total ? data.AllClears : data.Clears(CharData.JobId.GetRoleId());
            clearCountWidget.BaseLine = this.BaseLine;
            clearCountWidget.YOffset = this.YOffset;
            return clearCountWidget.Draw();
        }
        else if (Total)
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
        return float.Max(clearCountWidget.GetMinWidth(), progWidget.GetMinWidth());
    }
}
