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
using TomestoneViewer.Character.Client.FFLogsClient;
using TomestoneViewer.Character.Encounter;
using TomestoneViewer.Controller;
using TomestoneViewer.GUI.Formatters;
using TomestoneViewer.GUI.Widgets;

namespace TomestoneViewer.GUI.Main;

public class Table
{
    private readonly PartyView partyView;
    private readonly SingleCharacterView characterView;
    private readonly LowLevelFFLogsClient lowLevelFFLogsClient;
    private readonly Func<bool> renderFFLogs;
    private readonly WindowsController mainWindowController;

    internal Table(WindowsController mainWindowController, Func<bool> renderFFLogs, LowLevelFFLogsClient lowLevelFFLogsClient)
    { 
        this.mainWindowController = mainWindowController;
        this.partyView = new(mainWindowController, renderFFLogs);
        this.characterView = new(mainWindowController, renderFFLogs);
        this.renderFFLogs = renderFFLogs;
        this.lowLevelFFLogsClient = lowLevelFFLogsClient;
    }

    public void Draw(bool partyView)
    {
        if (partyView)
        {
            this.partyView.Draw();
        }
        else
        {
            this.characterView.Draw();
        }

        if (renderFFLogs() && !lowLevelFFLogsClient.CredentialsValid)
        {
            ImGui.TextColored(new Vector4(1, 0, 0, 1), "Credentials to FF Logs not set");
            ImGui.SameLine();
            if(ImGui.Button("Fix"))
            {
                mainWindowController.OpenConfig();
            }
        }
    }

    private void DrawEncounterStatus(CharData character, Location location, Vector2 cellStart, Vector2 cellEnd, int index)
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
        // if (this.renderFFLogs())
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
                else if (ffLogsData.Data != null && index != -1)
                {
                    if (ffLogsData.Data.ClearsPerJob.Count > 0)
                    {
                        ImGui.TableNextColumn();
                        ImGui.TableNextColumn();
                    }
                }

                Service.Fonts.DefaultSmaller.Pop();
            }
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
                    ImGui.TextUnformatted($"{TimeFormatters.FormatTimeRelative(lockout.Timestamp.Value.ToDateTime(TimeOnly.MinValue))}");
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
                ImGui.Text(TimeFormatters.FormatTimeRelative(clear.DateTime.Value));
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

        // if (this.renderFFLogs.Invoke())
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
                        // DrawKillsCount(ffData.ClearsPerJob[job], textY);
                        ImGui.TableNextColumn();
                        Util.ConditionalSeparator(firstRow);
                        ImGui.SetCursorPosY(textY);
                        Util.RightAlignText($"{TimeFormatters.FormatTimeRelative(ffData.ClearsPerJob[job].LastClear)}", null, width - ImGui.GetCursorPosX());
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
        // if (this.renderFFLogs())
        {
            size += FFLogsStatusWidth();
        }
        size += ImGui.GetStyle().ItemSpacing.X;
        return size;
    }
}
