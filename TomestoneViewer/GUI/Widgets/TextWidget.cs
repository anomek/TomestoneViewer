using System;
using System.Numerics;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;

namespace TomestoneViewer.GUI.Widgets;

internal class TextWidget : IWidget
{
    private float? compiledWidth = null;

    public enum AlignType
    {
        Left,
        Center,
        Right,
    }

    public string Text { get; set; } = string.Empty;

    public float? MinWidth { get; set; }

    public AlignType Align { get; set; } = AlignType.Left;

    public Func<IFontHandle> Font { get; set; } = () => Service.Fonts.Default;

    public bool CompileSeString { get; set; } = false;

    public float YOffset { get; internal set; } = 0;

    public float? BaseLine { get; set; } = null;

    public Vector2 Draw()
    {
        this.Font().Push();
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + this.YOffset);
        Util.ApplyBaseline(this.BaseLine);
        if (this.CompileSeString)
        {
            if (this.compiledWidth.HasValue)
            {
                switch (this.Align)
                {
                    case AlignType.Left:
                        break;
                    case AlignType.Center:
                        Util.CenterCursor(this.compiledWidth.Value, this.MinWidth);
                        break;
                    case AlignType.Right:
                        Util.RightAlignCursor(this.compiledWidth.Value, this.MinWidth);
                        break;
                }
            }

            this.compiledWidth = ImGuiHelpers.CompileSeStringWrapped(this.Text).Size.X;
        }
        else
        {
            switch (this.Align)
            {
                case AlignType.Left:
                    ImGui.TextUnformatted(this.Text);
                    break;
                case AlignType.Center:
                    Util.CenterText(this.Text, null, this.MinWidth);
                    break;
                case AlignType.Right:
                    Util.RightAlignText(this.Text, null, this.MinWidth);
                    break;
            }
        }

        this.Font().Pop();

        return default;
    }

    public float GetMinWidth()
    {
        if (this.CompileSeString)
        {
            return float.Max(this.MinWidth.GetValueOrDefault(0), this.compiledWidth.GetValueOrDefault(5000));
        }
        else
        {
            return this.MinWidth.GetValueOrDefault(0);
        }
    }
}
