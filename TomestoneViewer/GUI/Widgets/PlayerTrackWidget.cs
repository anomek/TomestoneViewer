using System.Numerics;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using TomestoneViewer.Character;

namespace TomestoneViewer.GUI.Widgets;

internal class PlayerTrackWidget : IWidget
{
    private float width = 0;

    internal CharData? CharData { get; set; }

    internal bool DrawDummy { get; set; } = false;

    public Vector2 Draw()
    {
        string text = string.Empty;
        if (this.CharData?.MainCharacterId != null)
        {
            text += $"Alt of {this.CharData.MainCharacterId.FullName}\n";
        }

        if (this.CharData?.PlayerTrackComment != null)
        {
            text += this.CharData?.PlayerTrackComment;
        }

        if (text != string.Empty)
        {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (1 * ImGuiHelpers.GlobalScale));
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
                ImGui.Text(string.Empty);
            }

            this.width = 0;
        }

        return default;
    }

    public float GetMinWidth()
    {
        return this.width;
    }
}
