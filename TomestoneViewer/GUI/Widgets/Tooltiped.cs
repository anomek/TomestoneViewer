using System.Numerics;

using Dalamud.Bindings.ImGui;

namespace TomestoneViewer.GUI.Widgets;

internal class Tooltiped : IWidget
{
    public required IWidget Main { get; set; }

    public required IWidget Tooltip { get; set; }

    public Vector2 Draw()
    {
        var result = this.Main.Draw();
        if (ImGui.IsItemHovered())
        {
            ImGui.SetNextWindowSize(new Vector2(this.Tooltip.GetMinWidth() + 20, 0));
            ImGui.BeginTooltip();
            this.Tooltip.Draw();
            ImGui.EndTooltip();
        }

        return result;
    }

    public float GetMinWidth()
    {
        return this.Main.GetMinWidth();
    }
}
