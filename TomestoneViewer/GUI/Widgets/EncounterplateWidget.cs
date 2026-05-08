using System;
using System.Numerics;

using Dalamud.Bindings.ImGui;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.GUI.Widgets;

internal class EncounterplateWidget : IWidget
{
    public required Location Location { get; set; }

    public Action<Location>? Callback { get; set; }

    public float Height { get; set; } = 0;

    public float YOffset { get; set; } = 0;

    public float? BaseLine { get; set; } = null;

    public Vector2 Draw()
    {
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + this.YOffset);
        if (ImGui.Selectable($"##EncounterplateWidget{this.Location.DisplayName}", false, ImGuiSelectableFlags.None, new Vector2(0, this.Height)))
        {
            this.Callback?.Invoke(this.Location);
        }

        ImGui.SameLine();
        Util.ApplyBaseline(this.BaseLine);
        ImGui.TextUnformatted($"{this.Location.DisplayName}");
        return default;
    }

    public float GetMinWidth()
    {
        return 0;
    }
}
