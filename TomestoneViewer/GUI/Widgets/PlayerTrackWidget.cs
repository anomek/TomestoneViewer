using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TomestoneViewer.Character;

namespace TomestoneViewer.GUI.Widgets;

internal class PlayerTrackWidget : IWidget
{
    internal CharData? CharData { get; set; }
    internal bool DrawDummy { get; set; } = false;

    private float width = 0;

    public Vector2 Draw()
    {
        var text = this.CharData?.PlayerTrackComment ?? string.Empty;
        if (text != string.Empty)
        {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1 * ImGuiHelpers.GlobalScale);
            var res = ImGuiHelpers.CompileSeStringWrapped("<icon(177)>");
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                Service.Fonts.DefaultSmallerStraight.Push();
                ImGui.TextUnformatted(text);
                Service.Fonts.DefaultSmallerStraight.Pop();
                ImGui.EndTooltip();
            }

            this.width = res.Size.X;
        }
        else
        {
            if (this.DrawDummy)
            {
                ImGui.Text("");
            }

            this.width = 0;
        }

        return default;
    }

    public float GetMinWidth()
    {
        return width;
    }
}
