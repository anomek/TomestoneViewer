using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TomestoneViewer.Character.Client;
using TomestoneViewer.Character.Encounter;
using static TomestoneViewer.Character.Encounter.EncounterData;

namespace TomestoneViewer.GUI.Widgets;
internal class TomestoneProgWidget : IWidget
{
    public LoadableData<TomestoneEncounterData> Data { get; set; }
    public IEncounterDataError? GenericTomestoneError { get; set; }
    public float? BaseLine { get; set; }
    public float YOffset { get; set; }

    public Vector2 Draw()
    {        
        var characterError = this.Data.Status.Error ?? this.GenericTomestoneError;

        var iconOffsetAdj = 2 * ImGuiHelpers.GlobalScale;
        if (characterError != null)
        {
            ImGui.PushFont(UiBuilder.IconFont);
            Util.ApplyBaseline(this.BaseLine, this.YOffset + iconOffsetAdj);
            Util.CenterText(characterError.Type.Symbol.ToIconString(), characterError.Type.Color);
            ImGui.PopFont();
            Util.SetHoverTooltip(characterError.Message);
        }
        else if (this.Data.Status.Loading)
        {
            ImGui.PushFont(UiBuilder.IconFont);
            Util.ApplyBaseline(this.BaseLine, this.YOffset + iconOffsetAdj);
            Util.Rotate(() => Util.CenterText(FontAwesomeIcon.CircleNotch.ToIconString(), new Vector4(.8f, .8f, .8f, 1)));
            ImGui.PopFont();
        }
        else if (this.Data.Data != null)
        {
            if (this.Data.Data.EncounterClear != null)
            {
                ImGui.PushFont(UiBuilder.IconFont);
                Util.ApplyBaseline(this.BaseLine, this.YOffset + iconOffsetAdj);
                Util.CenterText(FontAwesomeIcon.CheckCircle.ToIconString(), new Vector4(0, 1, 0, 1));
                ImGui.PopFont();
            }
            else if (this.Data.Data?.Progress != null)
            {
                Service.Fonts.ProgressFont.Push();
                Util.ApplyBaseline(this.BaseLine, this.YOffset + 1 * ImGuiHelpers.GlobalScale);
                Util.CenterText(this.Data.Data.Progress.ToString(), new Vector4(1, .7f, .1f, 1));
                Service.Fonts.ProgressFont.Pop();
            }
            else
            {
                ImGui.PushFont(UiBuilder.IconFont);
                Util.ApplyBaseline(this.BaseLine, this.YOffset + iconOffsetAdj);
                Util.CenterText(FontAwesomeIcon.MinusCircle.ToIconString(), new Vector4(.5f, .5f, .5f, 1));
                ImGui.PopFont();
            }
        }

        return default;
    }

    public float GetMinWidth()
    {
        return 0;
    }
}
