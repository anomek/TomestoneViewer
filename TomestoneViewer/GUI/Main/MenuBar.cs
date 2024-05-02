using System.Numerics;

using Dalamud.Interface;
using Dalamud.Interface.Utility;
using ImGuiNET;
using TomestoneViewer.Manager;

namespace TomestoneViewer.GUI.Main;

public class MenuBar
{
    public static void Draw()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(6 * ImGuiHelpers.GlobalScale, ImGui.GetStyle().ItemSpacing.Y));

        if (ImGui.BeginMenuBar())
        {
            // ------------------
            // Toogle view button
            // ------------------
            var swapViewIcon = Service.MainWindow.IsPartyView ? FontAwesomeIcon.User : FontAwesomeIcon.Users;
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.MenuItem(swapViewIcon.ToIconString()))
            {
                Service.MainWindow.IsPartyView = !Service.MainWindow.IsPartyView;
                if (Service.MainWindow.IsPartyView)
                {
                    Service.CharDataManager.UpdatePartyMembers();
                }
            }

            ImGui.PopFont();
            Util.SetHoverTooltip(Service.MainWindow.IsPartyView ? "Swap to single view" : "Swap to party view");

            //---------------
            // Align to right
            //---------------
            ImGui.PushFont(UiBuilder.IconFont);
            var x = ImGui.GetContentRegionAvail().X
                - ImGui.CalcTextSize(FontAwesomeIcon.Cog.ToIconString()).X
                - ImGui.CalcTextSize(FontAwesomeIcon.History.ToIconString()).X
                - (ImGui.GetStyle().ItemSpacing.X * 2.5f); // why 2.5f works here? no idea, but it looks ok

            if (x > 0)
            {
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + x);
            }

            ImGui.PopFont();

            //---------------
            // History button
            //---------------
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.MenuItem(FontAwesomeIcon.History.ToIconString()))
            {
                ImGui.OpenPopup("##History");
            }

            ImGui.PopFont();
            Util.SetHoverTooltip("History");

            DrawHistoryPopup();

            //---------------------
            // Configuration button
            //---------------------
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.MenuItem(FontAwesomeIcon.Cog.ToIconString()))
            {
                Service.ConfigWindow.Toggle();
            }

            ImGui.PopFont();
            Util.SetHoverTooltip("Configuration");

            ImGui.EndMenuBar();
        }

        ImGui.PopStyleVar();
    }

    public static void DrawHistoryPopup()
    {
        if (!ImGui.BeginPopup("##History", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.AlwaysAutoResize))
        {
            return;
        }

        var history = Service.HistoryManager.History;
        if (history.Count != 0)
        {
            var tableHeight = 12 * (25 * ImGuiHelpers.GlobalScale);
            if (history.Count < 12)
            {
                tableHeight = -1;
            }

            if (ImGui.BeginTable("##HistoryTable", 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY, new Vector2(-1, tableHeight)))
            {
                for (var i = 0; i < history.Count; i++)
                {
                    if (i != 0)
                    {
                        ImGui.TableNextRow();
                    }

                    ImGui.TableNextColumn();

                    var historyEntry = history[i];
                    if (ImGui.Selectable($"##PartyListSel{i}", false, ImGuiSelectableFlags.SpanAllColumns, new Vector2(0, 25 * ImGuiHelpers.GlobalScale)))
                    {
                        Service.CharDataManager.SetCharacter(CharSelector.SelectByName(historyEntry.FirstName, historyEntry.LastName, historyEntry.WorldName));
                        ImGui.CloseCurrentPopup();
                    }

                    ImGui.SameLine();
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text($"{historyEntry.LastSeen.ToShortDateString()} {historyEntry.LastSeen.ToShortTimeString()}");

                    ImGui.TableNextColumn();

                    ImGui.Text($"{historyEntry.FirstName} {historyEntry.LastName}");

                    ImGui.TableNextColumn();

                    ImGui.Text(historyEntry.WorldName);

                    ImGui.SameLine();
                    ImGui.Dummy(new Vector2(ImGui.GetStyle().ScrollbarSize));
                }

                ImGui.EndTable();
            }
        }
        else
        {
            ImGui.Text("No history");
        }

        ImGui.EndPopup();
    }
}
