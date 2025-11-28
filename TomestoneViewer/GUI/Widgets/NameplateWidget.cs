using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomestoneViewer.Character;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using TomestoneViewer.Character;
using TomestoneViewer.Character.Encounter;
using TomestoneViewer.Controller;
using TomestoneViewer.GUI.Formatters;
using TomestoneViewer.GUI.Widgets;
using Serilog;

namespace TomestoneViewer.GUI.Widgets;
internal class NameplateWidget : IWidget
{

    public CharData? CharData { get; set; }
    public Action<CharData>? Callback;

    public float BaseLine { get; set; } = 0;

    public Vector2 Draw()
    {
        if (this.CharData == null)
        {
            return default;
        }

        var jobIcon = this.CharData.JobId.Icon;

        if (ImGui.Selectable($"##PartyListCharacterSelectable{this.CharData.CharId.ToString()}", false, ImGuiSelectableFlags.None, new Vector2(0, jobIcon.Size)))
        {
            this.Callback?.Invoke(this.CharData);
        }

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
        ImGui.SameLine();
        jobIcon.Draw();
        ImGui.PopStyleVar();

        ImGui.SameLine();
        this.BaseLine = (this.CharData.JobId.Icon.Size + Util.XHeight()) / 2;
        Util.ApplyBaseline(BaseLine);
        ImGui.Text($" {CharData.CharId.FullName}");
        ImGui.SameLine();
        ImGuiHelpers.ScaledDummy(50, 1);

        // FIXME: at some point we may need real value
        return default;
    }

    public float GetMinWidth()
    {
        float size = 0;
        size += this.CharData?.JobId.Icon.Size ?? 0;
        size += ImGui.CalcTextSize(CharData.CharId.FullName).X;
        size += 30 * ImGuiHelpers.GlobalScale;
        return size;
    }
}
