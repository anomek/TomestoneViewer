using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using System;
using System.Linq;
using System.Numerics;
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
        this.DrawEncounterComboMenu();

        this.DrawPartyLayout();
    }

    private void DrawPartyLayout()
    {
        var currentParty = Service.CharDataManager.PartyMembers;
        if (ImGui.BeginTable(
              "##MainWindowTablePartyView",
              2,
              ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("name", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("status", ImGuiTableColumnFlags.WidthFixed, MaxStatusWidth());

            ImGui.TableNextRow();
            ImGui.TableNextRow();
            var i = 0;
            foreach (var charData in currentParty)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();

                int extraPixelsHeight = 4;
                var jobIcon = charData.JobId.Icon;
                ImGui.PushFont(UiBuilder.IconFont);
                var statusY = ImGui.GetCursorPosY() + (jobIcon.Size - ImGui.CalcTextSize(FontAwesomeIcon.QuestionCircle.ToIconString()).Y - extraPixelsHeight) / 2;
                ImGui.PopFont();

                if (ImGui.Selectable($"##PartyListCharacterSelectable{i}", false, ImGuiSelectableFlags.None, new Vector2(0, jobIcon.Size - extraPixelsHeight)))
                {
                    this.mainWindowController.OpenCharacter(CharSelector.SelectById(charData.CharId));
                }

                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
                ImGui.SameLine();
                jobIcon.Draw();
                ImGui.PopStyleVar();

                ImGui.SameLine();
                var middleCursorPosY = ImGui.GetCursorPosY() + (jobIcon.Size / 2) - (ImGui.GetFontSize() / 2) - (extraPixelsHeight /2);
                ImGui.SetCursorPosY(middleCursorPosY);

                ImGui.Text($"{charData.CharId.FullName}");
                ImGui.SameLine();

                ImGui.TableNextColumn();
                ImGui.SetCursorPosY(statusY);
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
                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                Service.Fonts.EncounterTypeHeader.Push();
                Util.ConditionalSeparator(!firstRow);
                ImGui.AlignTextToFramePadding();
                ImGui.TextUnformatted($" {category.DisplayName}");
                Service.Fonts.EncounterTypeHeader.Pop();

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
                    if (ImGui.Selectable($"  {location.DisplayName}##EncounterListSelectable{location.DisplayName}"))
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

    private static void DrawEncounterStatus(CharData character, Location location)
    {
        var encounterData = character.EncounterData[location].Tomestone;
        var characterError = character.EncounterData[location].Tomestone.Status.Error ?? character.CharError;
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
        else if (encounterData.Data != null)
        {
            if (encounterData.Data.EncounterClear != null)
            {
                ImGui.PushFont(UiBuilder.IconFont);
                Util.CenterText(FontAwesomeIcon.CheckCircle.ToIconString(), new Vector4(0, 1, 0, 1));
                ImGui.PopFont();

                if (IsItemHoveredAndOpenLinkOnDoubleClick(character, location) && encounterData.Data.EncounterClear.HasInfo)
                {
                    var clear = encounterData.Data.EncounterClear;
                    ImGui.BeginTooltip();
                    DoubleClickToOpenOnTomestoneText();

                    ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
                    if (clear.DateTime != null)
                    {
                        Service.Fonts.ClearedOnHeader.Push();
                        ImGui.Text("cleared on ");
                        ImGui.SameLine();
                        ImGui.Text(clear.DateTime.Value.ToString("yyyy-MM-dd"));
                        Service.Fonts.ClearedOnHeader.Pop();
                        ImGui.Text(FormatTimeRelative(clear.DateTime.Value));
                    }

                    var completionWeek = clear.CompletionWeek;
                    completionWeek ??= clear.CalculateCompletionWeek(location.Category.ReleaseDate);

                    if (completionWeek != null)
                    {
                        var start = ImGui.CalcTextSize("X").Y;
                        Service.Fonts.DefaultSmaller.Push();
                        var end = ImGui.CalcTextSize("X").Y;
                        ImGui.SameLine();
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + start - end);
                        ImGui.Text($"   ({completionWeek})");
                        Service.Fonts.DefaultSmaller.Pop();
                    }

                    ImGui.PopStyleVar();
                    ImGui.EndTooltip();
                }
            }
            else if (encounterData.Data?.Progress != null)
            {
                Service.Fonts.ProgressFont.Push();
                Util.CenterText(encounterData.Data.Progress.ToString(), new Vector4(1, .7f, .1f, 1));
                Service.Fonts.ProgressFont.Pop();

                if (IsItemHoveredAndOpenLinkOnDoubleClick(character, location))
                {
                    ImGui.BeginTooltip();
                    var align = ImGui.GetCursorPosX() + ImGui.CalcTextSize("P8 88.88% ").X;

                    DoubleClickToOpenOnTomestoneText();

                    foreach (var lockout in encounterData.Data.Progress.Lockouts)
                    {
                        var jobIcon = lockout.Job.SmallIcon;
                        jobIcon.Draw();
                        ImGui.SameLine();
                        Service.Fonts.ProgressFont.Push();
                        ImGui.TextUnformatted($"{lockout.Percent}");
                        Service.Fonts.ProgressFont.Pop();
                        if (lockout.Timestamp.HasValue)
                        {
                            var start = ImGui.CalcTextSize("X").Y;
                            Service.Fonts.DefaultSmaller.Push();
                            var end = ImGui.CalcTextSize("X").Y;
                            ImGui.SameLine();
                            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + start - end);
                            ImGui.SetCursorPosX(align + jobIcon.Size);
                            ImGui.TextUnformatted($"{FormatTimeRelative(lockout.Timestamp.Value.ToDateTime(TimeOnly.MinValue))}");
                            Service.Fonts.DefaultSmaller.Pop();
                        }
                    }

                    ImGui.EndTooltip();
                }
            }
            else
            {
                ImGui.PushFont(UiBuilder.IconFont);
                Util.CenterText(FontAwesomeIcon.MinusCircle.ToIconString(), new Vector4(.5f, .5f, .5f, 1));
                ImGui.PopFont();
            }
        }

        // TODO: else

#if DEBUG
        var ffLogsData = character.EncounterData[location].FFLogs;
        if (ffLogsData != null && ffLogsData.Data != null && ffLogsData.Data.ClearsPerJob.ContainsKey(JobId.Empty))
        {
            ImGui.SameLine();
            ImGui.Text($"{ffLogsData.Data.ClearsPerJob[JobId.Empty].ThisExpansion}");
        }
#endif
    }

    private static bool IsItemHoveredAndOpenLinkOnDoubleClick(CharData character, Location location)
    {
        var hovered = ImGui.IsItemHovered();
        if (hovered && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
        {
            Util.OpenLink(character.Links.EncounterActivity(location.Tomestone));
        }

        return hovered;
    }

    private static void DoubleClickToOpenOnTomestoneText()
    {
        Service.Fonts.TooltipDescription.Push();
        ImGui.TextUnformatted("\u00AB double click to see on tomestone.gg \u00BB");
        Service.Fonts.TooltipDescription.Pop();
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
