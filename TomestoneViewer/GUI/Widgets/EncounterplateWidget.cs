using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.GUI.Widgets;

internal class EncounterplateWidget : IWidget
{
    public Location Location { get; set; }
    public Action<Location>? Callback { get; set; }
    public float Height { get; set; } = 0;

    public float YOffset { get; set; } = 0;
    public float? BaseLine { get; set; } = null;

    public Vector2 Draw()
    {
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + YOffset);
        if (ImGui.Selectable($"##EncounterplateWidget{Location.DisplayName}", false, ImGuiSelectableFlags.None, new Vector2(0, this.Height)))
        {
            this.Callback?.Invoke(this.Location);
        }

        ImGui.SameLine();
        ImGui.SetCursorPosY(ImGui.GetCursorPosY());
        Util.ApplyBaseline(this.BaseLine);
        ImGui.TextUnformatted($"{Location.DisplayName}");
        return default;
    }

    public float GetMinWidth()
    {
        return 0;
    }
}
