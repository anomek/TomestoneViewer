using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using TomestoneViewer.Manager;
using TomestoneViewer.Model;
using ImGuiNET;

namespace TomestoneViewer;

public class Util
{
    public static bool DrawButtonIcon(FontAwesomeIcon icon, Vector2? size = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        if (size != null)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, size.Value);
        }

        var ret = ImGui.Button(icon.ToIconString());

        if (size != null)
        {
            ImGui.PopStyleVar();
        }

        ImGui.PopFont();

        return ret;
    }

    public static bool DrawDisabledButton(string label, bool isDisabled)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, isDisabled ? 0.5f : 1.0f);
        var ret = ImGui.Button(label);
        ImGui.PopStyleVar();

        return ret;
    }

    public static void DrawHelp(string helpMessage)
    {
        ImGui.SameLine();
        ImGui.TextColored(ImGuiColors.DalamudGrey, "(?)");

        SetHoverTooltip(helpMessage);
    }

    public static void IncList<T>(List<T> list, int index)
    {
        ArgumentNullException.ThrowIfNull(list);
        var indexA = Mod(index, list.Count);
        var indexB = Mod(index - 1, list.Count);
        (list[indexA], list[indexB]) = (list[indexB], list[indexA]);
    }

    public static void DecList<T>(List<T> list, int index)
    {
        var indexA = Mod(index, list.Count);
        var indexB = Mod(index + 1, list.Count);
        (list[indexA], list[indexB]) = (list[indexB], list[indexA]);
    }

    public static Vector4 GetLogColor(float? log)
    {
        var color = log switch
        {
            < 0 => new Vector4(255, 255, 255, 255),
            < 25 => new Vector4(102, 102, 102, 255),
            < 50 => new Vector4(30, 255, 0, 255),
            < 75 => new Vector4(0, 112, 255, 255),
            < 95 => new Vector4(163, 53, 238, 255),
            < 99 => new Vector4(255, 128, 0, 255),
            < 100 => new Vector4(226, 104, 168, 255),
            100 => new Vector4(229, 204, 128, 255),
            _ => new Vector4(255, 255, 255, 255),
        };

        return color / 255;
    }

    public static void CenterCursor(float width)
    {
        var offset = (ImGui.GetContentRegionAvail().X - width) / 2;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);
    }

    public static void CenterCursor(string text)
    {
        CenterCursor(ImGui.CalcTextSize(text, true).X);
    }

    public static void CenterText(string text, Vector4? color = null)
    {
        CenterCursor(text);

        color ??= ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.Text));
        ImGui.PushStyleColor(ImGuiCol.Text, color.Value);
        ImGui.TextUnformatted(text);
        ImGui.PopStyleColor();
    }

    public static void CenterError(CharacterError error)
    {
        CenterText(GetErrorMessage(error), GetErrorColor(error));
    }

    public static void CenterTextWithError(string text, CharData charData)
    {
        CenterText(text, charData.CharError != null ? GetErrorColor(charData.CharError.Value) : null);

        if (charData.CharError != null)
        {
            SetHoverTooltip(GetErrorMessage(charData.CharError.Value));
        }
    }

    public static void CenterSelectableError(CharacterError error, string hover)
    {
        CenterSelectable(GetErrorMessage(error), GetErrorColor(error));
        SetHoverTooltip(hover);
    }

    public static void CenterSelectableWithError(string text, CharData charData)
    {
        CenterSelectable(text, charData.CharError != null ? GetErrorColor(charData.CharError.Value) : null);
        if (charData.CharError != null)
        {
            SetHoverTooltip(GetErrorMessage(charData.CharError.Value));
        }
    }

    public static bool CenterSelectable(string text, Vector4? color = null)
    {
        CenterCursor(text);

        color ??= ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.Text));
        ImGui.PushStyleColor(ImGuiCol.Text, color.Value);
        var ret = ImGui.Selectable(text, false, ImGuiSelectableFlags.None, ImGui.CalcTextSize(text, true));
        ImGui.PopStyleColor();

        return ret;
    }

    public static void SelectableWithError(string text, CharData charData)
    {
        var color = charData.CharError != null
                        ? GetErrorColor(charData.CharError.Value)
                        : ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.Text));
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        ImGui.Selectable(text);
        ImGui.PopStyleColor();
        if (charData.CharError != null)
        {
            SetHoverTooltip(GetErrorMessage(charData.CharError.Value));
        }
    }

    public static void SetHoverTooltip(string tooltip)
    {
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.TextUnformatted(tooltip);
            ImGui.EndTooltip();
        }
    }

    public static int Mod(int x, int m) => ((x % m) + m) % m;

    public static string? GetFormattedLog(float? value, int nbOfDecimalDigits)
    {
        if (value == null)
        {
            return null;
        }

        if (value > 100.0f)
        {
            return "100";
        }

        var magnitude = Math.Pow(10, nbOfDecimalDigits);
        return (Math.Truncate(magnitude * value.Value) / magnitude).ToString("F" + nbOfDecimalDigits);
    }

    public static void OpenFFLogsLink(CharData charData)
    {
        // TODO:
        // OpenLink($"https://fflogs.com/character/{CharDataManager.GetRegionName(charData.WorldName)}/{charData.WorldName}/{charData.FirstName} {charData.LastName}");
    }

    public static void OpenTomestoneLink(CharData charData)
    {
        OpenLink($"https://tomestone.gg/character-name/{charData.CharId.World}/{charData.CharId.FullName}");
    }

    public static void OpenLink(string link)
    {
        Dalamud.Utility.Util.OpenLink(link);
    }

    public static void LinkOpenOrPopup(CharData charData)
    {
        if (ImGui.BeginPopupContextItem($"##LinkPopup{charData.CharId}{charData.GetHashCode()}", ImGuiPopupFlags.MouseButtonLeft))
        {
            OpenTomestoneLink(charData);
            ImGui.CloseCurrentPopup();
            return;
        }
    }

    public static unsafe SeString ReadSeString(byte* ptr)
    {
        var offset = 0;
        while (true)
        {
            var b = *(ptr + offset);
            if (b == 0)
            {
                break;
            }

            offset += 1;
        }

        var bytes = new byte[offset];
        Marshal.Copy(new nint(ptr), bytes, 0, offset);
        return SeString.Parse(bytes);
    }

    public static void UpdateDelayed(Stopwatch stopwatch, TimeSpan delayTime, Action function)
    {
        if (stopwatch.IsRunning && stopwatch.Elapsed >= delayTime)
        {
            stopwatch.Stop();
            stopwatch.Reset();
        }

        if (!stopwatch.IsRunning)
        {
            stopwatch.Start();
            function();
        }
    }

    public static string GetErrorMessage(CharacterError error)
    {
        return error switch
        {
            CharacterError.CharacterNotFoundTomestone => "Character not found on Tomestone.gg",
            CharacterError.CharacterNotFound => "Character not found",
            CharacterError.ClipboardError => "Couldn't get clipboard text",
            CharacterError.GenericError => "An error occured, please try again",
            CharacterError.HiddenLogs => "Logs are hidden",
            CharacterError.InvalidTarget => "Not a valid target",
            CharacterError.InvalidWorld => "World not supported or invalid",
            CharacterError.MalformedQuery => "Malformed GraphQL query.",
            CharacterError.MissingInputs => "Please fill first name, last name, and world",
            CharacterError.NetworkError => "Network error",
            CharacterError.OutOfPoints => "Ran out of API points, see Layout tab in config for more info.",
            CharacterError.Unauthenticated => "API Client not valid, check config",
            CharacterError.Unreachable => "Could not reach FF Logs servers",
            CharacterError.WorldNotFound => "World not found",
            _ => "If you see this, something went wrong",
        };
    }

    public static bool ShouldErrorBeClickable(CharData charData)
    {
        return charData.CharError is
                   CharacterError.CharacterNotFoundTomestone
                   or CharacterError.GenericError
                   or CharacterError.HiddenLogs
                   or CharacterError.MalformedQuery
                   or CharacterError.NetworkError
                   or CharacterError.OutOfPoints
                   or CharacterError.Unreachable;
    }

    public static Vector4 GetErrorColor(CharacterError error)
    {
        if (error is CharacterError.HiddenLogs)
        {
            return ImGuiColors.DalamudYellow;
        }

        return ImGuiColors.DalamudRed;
    }

    public static int MathMod(int a, int b)
    {
        return (Math.Abs(a * b) + a) % b;
    }
}
