using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomestoneViewer.GUI;

internal class Fonts : IDisposable
{
    public IFontHandle Default { get; private init; }
    public IFontHandle DefaultSmaller { get; private init; }

    public IFontHandle EncounterTypeHeader { get; private init; }

    public IFontHandle TooltipDescription { get; private init; }

    public IFontHandle ClearedOnHeader { get; private init; }

    public IFontHandle ProgressFont { get; private init; }

    internal Fonts()
    {
        this.Default = this.From(GameFontFamilyAndSize.Axis12);
        var defaultSmaller = new GameFontStyle(GameFontFamily.Axis, 13);
        defaultSmaller.Italic = true;

        this.DefaultSmaller = Service.Interface.UiBuilder.FontAtlas.NewGameFontHandle(defaultSmaller);
        this.EncounterTypeHeader = this.From(GameFontFamilyAndSize.MiedingerMid12);
        var tooltipDescriptionStyle = new GameFontStyle(GameFontFamily.Axis, 13);
        tooltipDescriptionStyle.Italic = true;
        this.TooltipDescription = Service.Interface.UiBuilder.FontAtlas.NewGameFontHandle(tooltipDescriptionStyle);
        this.ClearedOnHeader = this.From(GameFontFamilyAndSize.Jupiter16);
       
        this.ProgressFont = Service.Interface.UiBuilder.FontAtlas.NewGameFontHandle(new GameFontStyle(GameFontFamily.TrumpGothic, 18));
    }

    private IFontHandle From(GameFontFamilyAndSize familyAndSize)
    {
        return Service.Interface.UiBuilder.FontAtlas.NewGameFontHandle(new GameFontStyle(familyAndSize));
    }

    public void Dispose()
    {
        this.Default.Dispose();
        this.DefaultSmaller.Dispose();
        this.EncounterTypeHeader.Dispose();
        this.TooltipDescription.Dispose();
        this.ClearedOnHeader.Dispose();
        this.ProgressFont.Dispose();
    }
}
