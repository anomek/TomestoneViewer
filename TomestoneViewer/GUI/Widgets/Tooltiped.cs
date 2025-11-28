using Dalamud.Bindings.ImGui;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
namespace TomestoneViewer.GUI.Widgets;
internal class Tooltiped : IWidget
{
    public required IWidget Main { get; set; }
    public required IWidget Tooltip { get; set; }

    public Vector2 Draw()
    {
        var result = Main.Draw();
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
        return Main.GetMinWidth();
    }
}
