using System.Numerics;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using TomestoneViewer.Character.Encounter;
using TomestoneViewer.GUI.Formatters;

namespace TomestoneViewer.GUI.Widgets;

internal class ClearCountWidget() : IWidget
{
    private float swordIconWidth = 5;

    internal FFLogsEncounterData.CClearCount? Clears { get; set; }

    internal float? BaseLine { get; set; }

    internal bool IncludeLastClear { get; set; } = false;

    internal float YOffset { get; set; }

    public Vector2 Draw()
    {
        if (this.Clears == null)
        {
            return default;
        }

        var total = this.Clears.Total;
        var thisExp = this.Clears.ThisExpansion;

        if (total > 0)
        {
            Service.Fonts.DefaultSmaller.Push();

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));

            Util.ApplyBaseline(this.BaseLine, this.YOffset);

            if (this.IncludeLastClear)
            {
                Util.RightAlignText(TimeFormatters.FormatTimeRelativeShort(this.Clears.LastClear), null, TextAfterIconWidth());
                ImGui.SameLine();
                Service.Fonts.DefaultSmallerStraight.Push();
                ImGui.TextUnformatted(" \xe031");
                Service.Fonts.DefaultSmallerStraight.Pop();
                ImGui.SameLine();
            }

            Util.RightAlignText($"{total}", null, TextAfterIconWidth());
            ImGui.SameLine();
            var res = ImGuiHelpers.CompileSeStringWrapped("<icon(117)>");
            this.swordIconWidth = res.Size.X;

            ImGui.PopStyleVar();
            Service.Fonts.DefaultSmaller.Pop();
        }

        return default;
    }

    public float GetMinWidth()
    {
        float size = 0;
        if (this.IncludeLastClear)
        {
            size += TextAfterIconWidth();
            size += LastClearClockIconWidth();
        }

        size += this.swordIconWidth;
        size += TextAfterIconWidth();
        return size;
    }

    private static float LastClearClockIconWidth()
    {
        float size = 0;
        Service.Fonts.DefaultSmallerStraight.Push();
        size += ImGui.CalcTextSize(" \xe031").X;
        Service.Fonts.DefaultSmallerStraight.Pop();
        return size;
    }

    private static float TextAfterIconWidth()
    {
        float size = 0;
        Service.Fonts.DefaultSmaller.Push();
        size += ImGui.CalcTextSize("1999d").X;
        Service.Fonts.DefaultSmaller.Pop();
        return size;
    }
}
