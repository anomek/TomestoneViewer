using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using TomestoneViewer.Character;
using TomestoneViewer.Character.Encounter;
using TomestoneViewer.Controller;

namespace TomestoneViewer.GUI.Main;

public class Table(MainWindowController mainWindowController, Func<bool> renderFFLogs)
{
    private readonly MainWindowController mainWindowController = mainWindowController;
    private readonly Func<bool> renderFFLogs = renderFFLogs;

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
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(0, 0));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(2 * ImGuiHelpers.GlobalScale, 0));
        if (ImGui.BeginTable(
              "##MainWindowTablePartyView",
              2,
              ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.BordersInnerH))
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
                var startY = ImGui.GetCursorPosY();

                var jobIcon = charData.JobId.Icon;

                if (ImGui.Selectable($"##PartyListCharacterSelectable{i}", false, ImGuiSelectableFlags.None, new Vector2(0, jobIcon.Size)))
                {
                    this.mainWindowController.OpenCharacter(CharSelector.SelectById(charData.CharId));
                }

                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
                ImGui.SameLine();
                jobIcon.Draw();
                ImGui.PopStyleVar();

                ImGui.SameLine();
                ImGui.SetCursorPosY(startY + (jobIcon.Size - ImGui.GetTextLineHeight()) / 2);
                ImGui.Text($"{charData.CharId.FullName}");

                ImGui.TableNextColumn();
                var cellStart = ImGui.GetCursorPos();
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.SetCursorPosY(startY + (jobIcon.Size - ImGui.GetTextLineHeight()) / 2);
                ImGui.PopFont();
                DrawEncounterStatus(
                    charData,
                    Service.CharDataManager.CurrentEncounter,
                    cellStart,
                    cellStart + new Vector2(MaxStatusWidth(), jobIcon.Size));
                ImGui.TableNextColumn();
                i++;
            }

            ImGui.EndTable();
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleVar();
    }

    private void DrawSingleView()
    {
        if (Service.CharDataManager.DisplayedChar == null || Service.CharDataManager.CharacterError != null)
        {
            return;
        }


        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(0, 0));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(2 * ImGuiHelpers.GlobalScale, 0));

        if (ImGui.BeginTable(
                    "##MainWindowTableSingleView",
                    2,
                    ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.RowBg)) // | ImGuiTableFlags.BordersOuterH 
        {
            ImGui.TableSetupColumn("name", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("status", ImGuiTableColumnFlags.WidthFixed, MaxStatusWidth());

            ImGui.PushFont(UiBuilder.IconFont);
            var rowPadding = 2 * ImGuiHelpers.GlobalScale;
            var baseRowHeight = ImGui.CalcTextSize(FontAwesomeIcon.CheckCircle.ToIconString()).Y;
            var rowHeight = baseRowHeight + (rowPadding * 2);
            ImGui.PopFont();

            var character = Service.CharDataManager.DisplayedChar;

            var firstRow = true;
            foreach (var category in Category.All())
            {
                // fake row to start to ensure dark background
                ImGui.TableNextRow();


                // category row
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Separator();

                Service.Fonts.EncounterTypeHeader.Push();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + rowPadding + 1);
                ImGui.TextUnformatted($" {category.DisplayName}");
                Service.Fonts.EncounterTypeHeader.Pop();

                ImGui.TableNextColumn();
                ImGui.Separator();

                firstRow = false;
                var counter = 0;
                foreach (var location in category.Locations)
                {
                    var rowExtraTopPadding = counter == 0 ? 1 : 0;
                    ImGui.TableNextRow(rowHeight + rowExtraTopPadding);
                    ImGui.TableNextColumn();
                    Util.ConditionalSeparator(counter == 0);
                    var rowTopYPos = ImGui.GetCursorPosY();

                    ImGui.SetCursorPosY(rowTopYPos + rowExtraTopPadding);
                    if (ImGui.Selectable($"  ##EncounterListSelectable{location.DisplayName}", false, ImGuiSelectableFlags.None, new Vector2(0, rowHeight)))
                    {
                        this.mainWindowController.OpenParty(location);
                    }

                    ImGui.SameLine();
                    ImGui.SetCursorPosY(rowTopYPos + rowExtraTopPadding + (rowHeight - ImGui.GetTextLineHeight()) / 2);
                    ImGui.TextUnformatted(location.DisplayName);

                    ImGui.TableNextColumn();
                    var cellStart = ImGui.GetCursorPos() + new Vector2(0, rowExtraTopPadding);
                    Util.ConditionalSeparator(counter == 0);
                    ImGui.SetCursorPosY(rowTopYPos + rowPadding + rowExtraTopPadding);
                    DrawEncounterStatus(character, location, cellStart, cellStart + new Vector2(MaxStatusWidth(), rowHeight));
                    counter++;
                }
            }

            ImGui.EndTable();
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleVar();
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

    private void DrawEncounterStatus(CharData character, Location location, Vector2 cellStart, Vector2 cellEnd)
    {
        var padding = new Vector2(2, 2) * ImGuiHelpers.GlobalScale;
        cellStart = cellStart + ImGui.GetWindowPos() + padding;
        cellEnd = cellEnd + ImGui.GetWindowPos() - padding;

        var encounterData = character.EncounterData[location].Tomestone;
        var characterError = character.EncounterData[location].Tomestone.Status.Error ?? character.GenericTomestoneError;
        var tomestoneInfoRegionAvailable = TomestoneStatusWidth();
        var statingY = ImGui.GetCursorPosY();


        if (ImGui.IsMouseHoveringRect(cellStart, cellEnd))
        {
            this.DrawEncounterStatusMouseOver(character, character.EncounterData[location], location);
        }


        if (characterError != null)
        {
            ImGui.PushFont(UiBuilder.IconFont);
            Util.CenterText(characterError.Type.Symbol.ToIconString(), characterError.Type.Color, tomestoneInfoRegionAvailable);
            ImGui.PopFont();
            Util.SetHoverTooltip(characterError.Message);
        }
        else if (encounterData.Status.Loading)
        {
            ImGui.PushFont(UiBuilder.IconFont);
            Util.Rotate(() => Util.CenterText(FontAwesomeIcon.CircleNotch.ToIconString(), new Vector4(.8f, .8f, .8f, 1), tomestoneInfoRegionAvailable));
            ImGui.PopFont();
        }
        else if (encounterData.Data != null)
        {
            if (encounterData.Data.EncounterClear != null)
            {
                ImGui.PushFont(UiBuilder.IconFont);
                Util.CenterText(FontAwesomeIcon.CheckCircle.ToIconString(), new Vector4(0, 1, 0, 1), tomestoneInfoRegionAvailable);
                ImGui.PopFont();


            }
            else if (encounterData.Data?.Progress != null)
            {
                Service.Fonts.ProgressFont.Push();
                Util.CenterText(encounterData.Data.Progress.ToString(), new Vector4(1, .7f, .1f, 1), tomestoneInfoRegionAvailable);
                Service.Fonts.ProgressFont.Pop();
            }
            else
            {
                ImGui.PushFont(UiBuilder.IconFont);
                Util.CenterText(FontAwesomeIcon.MinusCircle.ToIconString(), new Vector4(.5f, .5f, .5f, 1), tomestoneInfoRegionAvailable);
                ImGui.PopFont();
            }
        }

        // TODO: else
        if (this.renderFFLogs())
        {
            if (!encounterData.Status.Loading && (characterError != null || encounterData.Data == null || encounterData.Data.Cleared))
            {
                ImGui.PushFont(UiBuilder.IconFont);
                var baseRowHeight = ImGui.GetTextLineHeight();
                ImGui.PopFont();
                Service.Fonts.DefaultSmaller.Push();

                var textY = statingY + baseRowHeight - ImGui.GetTextLineHeight() + (1 * ImGuiHelpers.GlobalScale);

                var ffLogsData = character.EncounterData[location].FFLogs;
                var fflogsError = character.EncounterData[location].FFLogs.Status.Error ?? character.GenericFFLogsError;

                if (fflogsError != null)
                {
                    ImGui.SameLine();
                    ImGui.SetCursorPosY(textY);
                    Util.RightAlignText(fflogsError.Type.Glyph, fflogsError.Type.Color, FFLogsNumberStatusWidth());
                    Util.SetHoverTooltip(fflogsError.Message);
                }
                else if (ffLogsData.Status.Loading)
                {
                    ImGui.SameLine();
                    ImGui.SetCursorPosY(textY);
                    Util.RightAlignCursor(".....", FFLogsNumberStatusWidth());
                    Util.Progress(".....");
                }
                else if (ffLogsData.Data != null)
                {

                    if (ffLogsData.Data.ClearsPerJob.Count > 0)
                    {
                        DrawKillsCount(ffLogsData.Data.AllClears, textY);
                    }
                }

                Service.Fonts.DefaultSmaller.Pop();
            }
        }
    }

    private static void DrawKillsCount(FFLogsEncounterData.CClearCount clears, float? alignment)
    {
        var total = clears.Total;
        var count = clears.ThisExpansion;
        if (total > 0)
        {
            Service.Fonts.DefaultSmaller.Push();
            ImGui.SameLine();
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
            if (alignment != null) ImGui.SetCursorPosY(alignment.Value);
            var countStr = count == 0 ? "\u2013" : $"{count}";
            Util.RightAlignText(countStr, null, FFLogsNumberStatusWidth());
            if (total > count)
            {
                ImGui.SameLine();
                if (alignment != null) ImGui.SetCursorPosY(alignment.Value);
                ImGui.Text("/");
                ImGui.SameLine();
                if (alignment != null) ImGui.SetCursorPosY(alignment.Value);
                Util.RightAlignText($"{total}", null, FFLogsNumberStatusWidth());
            }

            ImGui.PopStyleVar();
            Service.Fonts.DefaultSmaller.Pop();
        }
    }

    private void DrawEncounterStatusMouseOver(CharData character, EncounterData data, Location location)
    {
        if (data.Tomestone.Data == null)
        {
            return;
        }

        if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
        {
            Util.OpenLink(character.Links.EncounterActivity(location.Tomestone));
        }

        ImGui.BeginTooltip();

        var align = ImGui.GetCursorPosX() + ImGui.CalcTextSize("P8 88.88% ").X;
        var encounterData = data.Tomestone;
        var clear = encounterData.Data.EncounterClear;

        Service.Fonts.TooltipDescription.Push();
        ImGui.TextUnformatted("\u00AB double click to see on tomestone.gg \u00BB");
        var width = ImGui.GetItemRectSize().X;
        Service.Fonts.TooltipDescription.Pop();

        if (encounterData.Data.Progress != null)
        {
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
        }

        if (clear != null)
        {
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
        }

        if (this.renderFFLogs.Invoke())
        {
            var ffData = data.FFLogs.Data;
            if (ffData != null && ffData.ClearsPerJob.Count > 0)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(1 * ImGuiHelpers.GlobalScale, 1 * ImGuiHelpers.GlobalScale));
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(2 * ImGuiHelpers.GlobalScale, 0));
                Service.Fonts.DefaultSmaller.Push();
                if (ImGui.BeginTable("##EncounterTooltipClearPerJobTable", 2))
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Separator();
                    ImGui.TextUnformatted("Clears\nin Dawntrail / Total");
                    ImGui.TableNextColumn();
                    ImGui.Separator();
                    Util.RightAlignText("\nLast Clear", null, width - ImGui.GetCursorPosX());

                    var firstRow = true;
                    foreach (var job in ffData.ClearsPerJob.OrderByDescending(c => c.Value.LastClear).Select(c => c.Key))
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        Util.ConditionalSeparator(firstRow);
                        job.SmallIcon.Draw();
                        ImGui.SameLine();
                        var textY = ImGui.GetCursorPosY() + job.SmallIcon.Size - ImGui.GetTextLineHeight();
                        DrawKillsCount(ffData.ClearsPerJob[job], textY);
                        ImGui.TableNextColumn();
                        Util.ConditionalSeparator(firstRow);
                        ImGui.SetCursorPosY(textY);
                        Util.RightAlignText($"{FormatTimeRelative(ffData.ClearsPerJob[job].LastClear)}", null, width - ImGui.GetCursorPosX());
                        firstRow = false;
                    }

                    ImGui.EndTable();
                }

                Service.Fonts.DefaultSmaller.Pop();
                ImGui.PopStyleVar();
                ImGui.PopStyleVar();
            }
        }

        ImGui.EndTooltip();
    }

    private static string FormatTimeRelative(DateTime timestamp)
    {
        var diff = DateTime.Now - timestamp;
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

    private static float TomestoneStatusWidth()
    {
        float size = 0;
        Service.Fonts.ProgressFont.Push();
        size += ImGui.CalcTextSize("99%% P9").X;
        Service.Fonts.ProgressFont.Pop();
        return size;
    }

    private static float FFLogsNumberStatusWidth()
    {
        float size = 0;
        Service.Fonts.DefaultSmaller.Push();
        size += ImGui.CalcTextSize("999").X;
        Service.Fonts.DefaultSmaller.Pop();
        return size;
    }

    private static float FFLogsShashStatusWidth()
    {
        float size = 0;
        Service.Fonts.DefaultSmaller.Push();
        size += ImGui.CalcTextSize("/").X;
        Service.Fonts.DefaultSmaller.Pop();
        return size;
    }

    private static float FFLogsStatusWidth()
    {
        return (2 * FFLogsNumberStatusWidth()) + FFLogsShashStatusWidth();
    }

    private float MaxStatusWidth()
    {
        float size = 0;
        size += TomestoneStatusWidth();
        if (this.renderFFLogs())
        {
            size += FFLogsStatusWidth();
        }
        size += ImGui.GetStyle().ItemSpacing.X;
        return size;
    }
}
