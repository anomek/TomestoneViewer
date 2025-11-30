using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomestoneViewer.Character.Encounter;
using TomestoneViewer.Controller;
using TomestoneViewer.GUI.Widgets;

namespace TomestoneViewer.GUI.Main;
internal class PartyView
{
    private readonly WindowsController mainWindowController;

    private readonly PartyTableView partyTableView;
    private readonly TextWidget tableLegend;

    public PartyView(WindowsController mainWindowController, Func<bool> ffLogsEnabled)
    {
        this.mainWindowController = mainWindowController;
        this.partyTableView = new(mainWindowController, ffLogsEnabled);
        this.tableLegend = new TextWidget()
        {
            Text = "last cleared, clears this expansions/clears in total",
            Font = () => Service.Fonts.DefaultSmaller,
            Align = TextWidget.AlignType.Right,
        };
    }

    public void Draw()
    {
        if (Util.DrawButtonIcon(FontAwesomeIcon.Redo))
        {
            this.mainWindowController.OpenParty();
        }

        Util.SetHoverTooltip("Refresh party state");
        ImGui.SameLine();

        //------------------------------
        // Encounter combo and favourite
        //------------------------------
        this.DrawEncounterComboMenu();
        var tableSize = this.partyTableView.Draw();
    }

    private void DrawEncounterComboMenu()
    {
        ImGui.SameLine();

        var minWidth = Location.All().Select(location => ImGui.CalcTextSize(location.DisplayName).X).Max()
                               + (30 * ImGuiHelpers.GlobalScale);

        var widthFromWindowSize = ImGui.GetContentRegionAvail().X;
        ImGui.SetNextItemWidth(Math.Max(minWidth, widthFromWindowSize));

        if (ImGui.BeginCombo("##EncounterLayoutCombo", $" {Service.CharDataManager.CurrentEncounter.DisplayName}", ImGuiComboFlags.HeightLargest))
        {
            ImGui.Separator();
            foreach (var category in Category.All())
            {
                foreach (var encounter in category.Locations)
                {
                    var name = encounter.DisplayName;

                    if (ImGui.Selectable($" {name}"))
                    {
                        this.mainWindowController.OpenParty(encounter);
                    }
                }

                ImGui.Separator();
            }

            ImGui.EndCombo();
        }
    }
}
