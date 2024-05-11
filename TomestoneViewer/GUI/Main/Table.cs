using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using Dalamud.Interface;
using Dalamud.Interface.Utility;
using ImGuiNET;
using TomestoneViewer.Character;
using TomestoneViewer.Character.Encounter;
using TomestoneViewer.Controller;

namespace TomestoneViewer.GUI.Main;

public class Table(MainWindowController mainWindowController)
{
    private readonly MainWindowController mainWindowController = mainWindowController;

    public void Draw(bool partyView)
    {
        if (partyView)
        {
            this.DrawPartyView();
        }
        else
        {
            this.DrawSingleView();
        }
    }

    private void DrawPartyView()
    {
        //--------------------
        // Refresh party state
        //--------------------
        if (Util.DrawButtonIcon(FontAwesomeIcon.Redo))
        {
            this.mainWindowController.OpenParty();
        }

        Util.SetHoverTooltip("Refresh party state");
        ImGui.SameLine();

        //------------------------------
        // Encounter combo and favourite
        //------------------------------
        DrawEncounterComboMenu();

        this.DrawPartyLayout();
    }

    private void DrawPartyLayout()
    {
        var currentParty = Service.CharDataManager.PartyMembers;
        if (ImGui.BeginTable(
              "##MainWindowTablePartyView",
              2,
              ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn("name", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("status", ImGuiTableColumnFlags.WidthFixed, MaxStatusWidth());

            ImGui.TableNextRow();
            ImGui.TableNextRow();
            var iconSize = (float)Math.Round(25 * ImGuiHelpers.GlobalScale); // round because of shaking issues
            var i = 0;
            foreach (var charData in currentParty)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                var icon = Service.GameDataManager.JobIconsManager.GetJobIcon(charData.JobId);
                if (icon != null)
                {
                    ImGui.Image(icon.ImGuiHandle, new Vector2(iconSize));
                }
                else
                {
                    ImGui.Text("(?)");
                }

                ImGui.SameLine();
                if (ImGui.Selectable($"{charData.CharId.FullName}##PartyListCharacterSelectable{i}"))
                {
                    this.mainWindowController.OpenCharacter(CharSelector.SelectById(charData.CharId));
                }

                ImGui.TableNextColumn();
                DrawEncounterStatus(
                    charData,
                    Service.CharDataManager.CurrentEncounter);
                ImGui.TableNextColumn();

                i++;
            }

            ImGui.EndTable();
        }
    }

    private void DrawSingleView()
    {
        if (Service.CharDataManager.DisplayedChar == null || Service.CharDataManager.CharacterError != null)
        {
            return;
        }

        if (ImGui.BeginTable(
                    "##MainWindowTableSingleView",
                    2,
                    ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn("name", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("status", ImGuiTableColumnFlags.WidthFixed, MaxStatusWidth());

            var minRowHeight = 21;
            var character = Service.CharDataManager.DisplayedChar;

            var firstRow = true;
            foreach (var category in Category.All())
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                Util.ConditionalSeparator(!firstRow);
                ImGui.AlignTextToFramePadding();
                ImGui.TextUnformatted(category.DisplayName);
                ImGui.TableNextColumn();
                Util.ConditionalSeparator(!firstRow);

                firstRow = false;
                var counter = 0;
                foreach (var location in category.Locations)
                {
                    ImGui.TableNextRow(ImGuiTableRowFlags.None, minRowHeight);
                    ImGui.TableNextColumn();
                    Util.ConditionalSeparator(counter == 0);
                    ImGui.AlignTextToFramePadding();
                    if (ImGui.Selectable($"{location.DisplayName}##EncounterListSelectable{location.DisplayName}"))
                    {
                        this.mainWindowController.OpenParty(location);
                    }

                    ImGui.TableNextColumn();
                    Util.ConditionalSeparator(counter == 0);
                    DrawEncounterStatus(character, location);

                    counter++;
                }
            }

            ImGui.EndTable();
        }
    }

    private static void DrawEncounterComboMenu()
    {
        ImGui.SameLine();

        var minWidth = Location.All().Select(location => ImGui.CalcTextSize(location.DisplayName).X).Max()
                               + (30 * ImGuiHelpers.GlobalScale);

        var widthFromWindowSize = ImGui.GetContentRegionAvail().X;
        ImGui.SetNextItemWidth(Math.Max(minWidth, widthFromWindowSize));

        if (ImGui.BeginCombo("##EncounterLayoutCombo", Service.CharDataManager.CurrentEncounter.DisplayName, ImGuiComboFlags.HeightLargest))
        {
            ImGui.Separator();
            foreach (var category in Category.All())
            {
                foreach (var encounter in category.Locations)
                {
                    var name = encounter.DisplayName;

                    if (ImGui.Selectable(name))
                    {
                        Service.CharDataManager.CurrentEncounter = encounter;
                    }
                }

                ImGui.Separator();
            }

            ImGui.EndCombo();
        }
    }

    private static void DrawEncounterStatus(CharData character, Location location)
    {
        var encounterData = character.EncounterData[location];
        var characterError = character.EncounterData[location].Status.Error ?? character.CharError;
        if (characterError != null)
        {
            ImGui.PushFont(UiBuilder.IconFont);
            Util.CenterText(characterError.Symbol.ToIconString(), characterError.Color);
            ImGui.PopFont();
            Util.SetHoverTooltip(characterError.Message);
        }
        else if (encounterData.Status.Loading)
        {
            ImGui.PushFont(UiBuilder.IconFont);
            Util.Rotate(() => Util.CenterText(FontAwesomeIcon.CircleNotch.ToIconString(), new Vector4(.8f, .8f, .8f, 1)));
            ImGui.PopFont();
        }
        else if (encounterData.EncouterProgress != null)
        {
            if (encounterData.EncouterProgress.EncounterClear != null)
            {
                ImGui.PushFont(UiBuilder.IconFont);
                Util.CenterText(FontAwesomeIcon.CheckCircle.ToIconString(), new Vector4(0, 1, 0, 1));
                ImGui.PopFont();

                if (ImGui.IsItemHovered() && encounterData.EncouterProgress.EncounterClear.HasInfo)
                {
                    var clear = encounterData.EncouterProgress.EncounterClear;
                    ImGui.BeginTooltip();
                    ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
                    if (clear.DateTime != null)
                    {
                        ImGui.Text("Cleared on ");
                        ImGui.SameLine();
                        ImGui.Text(clear.DateTime.Value.ToString("yyyy-MM-dd"));
                        ImGui.SetWindowFontScale(.95f);
                        ImGui.Text(FormatTimeRelative(clear.DateTime.Value));
                        ImGui.SetWindowFontScale(1);
                    }

                    var completionWeek = clear.CompletionWeek;
                    completionWeek ??= clear.CalculateCompletionWeek(location.Category.ReleaseDate);

                    if (completionWeek != null)
                    {
                        ImGui.SameLine();
                        ImGui.SetWindowFontScale(.95f);
                        ImGui.Text($" ({completionWeek})");
                        ImGui.SetWindowFontScale(1);
                    }

                    ImGui.PopStyleVar();
                    ImGui.EndTooltip();
                }
            }
            else if (encounterData.EncouterProgress.Progress != null)
            {
                Util.CenterText(encounterData.EncouterProgress.Progress, new Vector4(1, .7f, .1f, 1));
            }
            else
            {
                ImGui.PushFont(UiBuilder.IconFont);
                Util.CenterText(FontAwesomeIcon.MinusCircle.ToIconString(), new Vector4(.5f, .5f, .5f, 1));
                ImGui.PopFont();
            }
        }

        // TODO: else
    }

    private static string FormatTimeRelative(DateTime timestamp)
    {
        var diff = DateTime.UtcNow - timestamp;
        int number;
        string unit;
        if (diff.TotalMinutes < 120)
        {
            number = (int)diff.TotalMinutes;
            unit = "minute";
        }
        else if (diff.TotalHours < 48)
        {
            number = (int)diff.TotalHours;
            unit = "hour";
        }
        else if (diff.TotalDays < 65)
        {
            number = (int)diff.TotalDays;
            unit = "day";
        }
        else if (diff.TotalDays < 366)
        {
            number = (int)(diff.TotalDays / 30.43);
            unit = "month";
        }
        else
        {
            number = (int)(diff.TotalDays / 365.25);
            unit = "year";
        }

        var plular = number == 1 ? string.Empty : "s";
        return $"{number} {unit}{plular} ago";
    }

    private static float MaxStatusWidth()
    {
        return ImGui.CalcTextSize("99.99%% P9").X + (ImGui.GetStyle().ItemSpacing.X * 2);
    }
}
