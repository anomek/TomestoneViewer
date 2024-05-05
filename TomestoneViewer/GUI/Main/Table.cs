using System;
using System.Linq;
using System.Numerics;

using Dalamud.Interface;
using Dalamud.Interface.Utility;
using ImGuiNET;
using TomestoneViewer.Character;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.GUI.Main;

public class Table
{
    private static readonly DateTime AnabeisosRelaseDate = new(2023, 5, 30);

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

    private static void DrawPartyViewWarning()
    {
        if (Service.CharDataManager.PartyMembers.Count == 0)
        {
            ImGui.Text("Use");
            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.SameLine();
            ImGui.Text(FontAwesomeIcon.Redo.ToIconString());
            ImGui.PopFont();
            ImGui.SameLine();
            ImGui.Text("to refresh the party state.");
        }
    }

    private void DrawPartyView()
    {
        //--------------------
        // Refresh party state
        //--------------------
        if (Util.DrawButtonIcon(FontAwesomeIcon.Redo))
        {
            Service.CharDataManager.UpdatePartyMembers();
        }

        Util.SetHoverTooltip("Refresh party state");
        ImGui.SameLine();

        //------------------------------
        // Encounter combo and favourite
        //------------------------------
        this.DrawEncounterComboMenu();

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
            foreach (var charData in currentParty)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                var icon = Service.GameDataManager.JobIconsManager.GetJobIcon(charData?.JobId ?? 0);
                if (icon != null)
                {
                    ImGui.Image(icon.ImGuiHandle, new Vector2(iconSize));
                }
                else
                {
                    ImGui.Text("(?)");
                }

                ImGui.SameLine();
                ImGui.Text(charData.CharId.FullName);
                ImGui.TableNextColumn();

                this.DrawEncounterStatus(
                    charData,
                    Service.CharDataManager.CurrentEncounterDisplayName,
                    EncounterLocation.CategoryFromLocationName(Service.CharDataManager.CurrentEncounterDisplayName));
                ImGui.TableNextColumn();
            }

            ImGui.EndTable();
        }
    }

    private void DrawSingleView()
    {
        if (Service.CharDataManager.CharacterError != null)
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
            foreach (var category in EncounterLocation.LOCATIONS)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                if (!firstRow) ImGui.Separator();
                ImGui.AlignTextToFramePadding();
                ImGui.TextUnformatted(category.DisplayName);
                ImGui.TableNextColumn();
                if (!firstRow) ImGui.Separator();

                if (firstRow) firstRow = false;

                int counter = 0;
                foreach (var location in category.Locations)
                {
                    ImGui.TableNextRow(ImGuiTableRowFlags.None, minRowHeight);
                    ImGui.TableNextColumn();
                    if (counter == 0) ImGui.Separator();
                    ImGui.AlignTextToFramePadding();
                    ImGui.TextUnformatted(location.DisplayName);
                    ImGui.TableNextColumn();
                    if (counter == 0) ImGui.Separator();
                    this.DrawEncounterStatus(character, location.DisplayName, category);

                    counter++;
                }
            }

            ImGui.EndTable();
        }
    }

    private void DrawEncounterComboMenu()
    {
        var allLocations = EncounterLocation.AllLocations();

        ImGui.SameLine();

        var minWidth = allLocations.Select(location => ImGui.CalcTextSize(location.DisplayName).X).Max()
                               + (30 * ImGuiHelpers.GlobalScale);

        var widthFromWindowSize = ImGui.GetContentRegionAvail().X;
        ImGui.SetNextItemWidth(Math.Max(minWidth, widthFromWindowSize));

        Service.CharDataManager.CurrentEncounterDisplayName ??= allLocations[0].DisplayName;
        var comboPreview = Service.CharDataManager.CurrentEncounterDisplayName;

        if (ImGui.BeginCombo("##EncounterLayoutCombo", comboPreview, ImGuiComboFlags.HeightLargest))
        {
            ImGui.Separator();
            foreach (var category in EncounterLocation.LOCATIONS)
            {
                foreach (var encounter in category.Locations)
                {
                    var name = encounter.DisplayName;

                    if (ImGui.Selectable(name))
                    {
                        Service.CharDataManager.CurrentEncounterDisplayName = encounter.DisplayName;
                    }
                }

                ImGui.Separator();
            }

            ImGui.EndCombo();
        }
    }

    private void DrawEncounterStatus(CharData character, string? locationDisplayName, EncounterLocation.Category category)
    {
        if (locationDisplayName == null)
        {
            return;
        }

        var encounterData = character.EncounterData[locationDisplayName];
        var characterError = character.EncounterData[locationDisplayName].Status.Error ?? character.CharError;
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
                    completionWeek ??= category.DisplayName == "Savage" ? clear.CalculateCompletionWeek(AnabeisosRelaseDate) : null;

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
        var number = 0;
        var unit = string.Empty;
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
