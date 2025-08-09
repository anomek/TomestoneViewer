using System;
using System.Numerics;
using System.Runtime.InteropServices;

using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface;
using Dalamud.Bindings.ImGui;
using TomestoneViewer.Character;

namespace TomestoneViewer;

public class Util
{
    public static void ConditionalSeparator(bool shouldDraw)
    {
        if (shouldDraw)
        {
            ImGui.Separator();
        }
    }

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

    public static void CenterError(IRenderableError error)
    {
        CenterText(error.Message, error.Color);
    }

    public static void CenterSelectableError(IRenderableError error)
    {
        CenterSelectable(error.Message, error.Color);
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

    public static bool SetHoverTooltip(string tooltip)
    {
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.TextUnformatted(tooltip);
            ImGui.EndTooltip();
            return true;
        }

        return false;
    }

    public static void OpenLink(string link)
    {
        Dalamud.Utility.Util.OpenLink(link);
    }

    public static void LinkOpenOrPopup(CharData charData)
    {
        var fflogsLink = charData.Links.FFLogs();
        if (fflogsLink == null)
        {
            if (SetHoverTooltip("Click to open on Tomestone.gg") && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                OpenLink(charData.Links.TomestoneMain());
            }
        }
        else
        {
            SetHoverTooltip("Click to open on ...");
            if (ImGui.BeginPopupContextItem($"##LinkPopup{charData.CharId}{charData.GetHashCode()}", ImGuiPopupFlags.MouseButtonLeft))
            {
                if (ImGui.Selectable("Tomestone.gg"))
                {
                    OpenLink(charData.Links.TomestoneMain());
                    ImGui.CloseCurrentPopup();
                }

                if (ImGui.Selectable("FFLogs"))
                {
                    OpenLink(fflogsLink);
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }
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

    public static void Rotate(Action action)
    {
        var rotationStartIndex = ImGui.GetWindowDrawList().VtxBuffer.Size;
        action.Invoke();
        var angle = DateTime.Now.Millisecond * 3.14 * 2 / 1000f;
        var sin = (float)Math.Sin(angle);
        var cos = (float)Math.Cos(angle);

        var buf = ImGui.GetWindowDrawList().VtxBuffer;
        if (buf.Size <= rotationStartIndex)
        {
            return;
        }

        var low = buf[rotationStartIndex].Pos;
        var high = buf[rotationStartIndex].Pos;
        for (var i = rotationStartIndex + 1; i < buf.Size; i++)
        {
            low = Vector2.Min(low, buf[i].Pos);
            high = Vector2.Max(high, buf[i].Pos);
        }

        var center = new Vector2((low.X + high.X) / 2, (low.Y + high.Y) / 2);
        center = ImRotate(center, sin, cos) - center;
        for (var i = rotationStartIndex; i < buf.Size; i++)
        {
            buf.Ref(i).Pos = ImRotate(buf[i].Pos, sin, cos) - center;
        }
    }

    private static Vector2 ImRotate(Vector2 vector, float sin, float cos)
    {
        return new Vector2((vector.X * cos) - (vector.Y * sin), (vector.X * sin) + (vector.Y * cos));
    }
}
