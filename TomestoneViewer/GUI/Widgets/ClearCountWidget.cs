using Dalamud.Bindings.ImGui;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.ImGuiSeStringRenderer;
using Dalamud.Interface.Utility;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using System;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TomestoneViewer.Character;
using TomestoneViewer.Character.Encounter;
using TomestoneViewer.Character.Encounter;
using TomestoneViewer.Controller;
using TomestoneViewer.GUI.Formatters;
using TomestoneViewer.GUI.Formatters;
using static Dalamud.Interface.Utility.Raii.ImRaii;

namespace TomestoneViewer.GUI.Widgets;

internal class ClearCountWidget() : IWidget
{

    internal FFLogsEncounterData.CClearCount? Clears { get; set; }
    internal float? BaseLine { get; set; }

    internal bool IncludeLastClear { get; set; } = false;
    internal float YOffset { get; set; }

    private float swordIconWidth = 5;

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

        // FIXME: at some point we may need real value
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
        size += swordIconWidth;
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
