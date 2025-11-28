using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;

namespace TomestoneViewer.GUI;

internal class Fonts : IDisposable
{
    private readonly Func<bool> useDefaultFonts;

    private readonly IReadOnlyDictionary<FontType, IFontHandle> fonts;

    public IFontHandle Default => this.FontHandle(FontType.Default);


    public IFontHandle DefaultSmallerStraight => this.FontHandle(FontType.DefaultSmallerStraight);

    public IFontHandle DefaultSmaller => this.FontHandle(FontType.DefaultSmaller);

    public IFontHandle EncounterTypeHeader => this.FontHandle(FontType.EncounterTypeHeader);

    public IFontHandle TooltipDescription => this.FontHandle(FontType.TooltipDescription);

    public IFontHandle ClearedOnHeader => this.FontHandle(FontType.ClearedOnHeader);

    public IFontHandle ProgressFont => this.FontHandle(FontType.ProgressFont);

    internal Fonts(Func<bool> useDefaultFonts)
    {
        var fonts = new Dictionary<FontType, IFontHandle>();
        this.useDefaultFonts = useDefaultFonts;

        fonts[FontType.Default] = this.From(GameFontFamilyAndSize.Axis12);

        fonts[FontType.DefaultSmallerStraight] = Service.Interface.UiBuilder.FontAtlas.NewGameFontHandle(new GameFontStyle(GameFontFamily.Axis, 13));
        var defaultSmaller = new GameFontStyle(GameFontFamily.Axis, 13);
        defaultSmaller.Italic = true;
        fonts[FontType.DefaultSmaller] = Service.Interface.UiBuilder.FontAtlas.NewGameFontHandle(defaultSmaller);
        fonts[FontType.EncounterTypeHeader] = this.From(GameFontFamilyAndSize.MiedingerMid12);
        var tooltipDescriptionStyle = new GameFontStyle(GameFontFamily.Axis, 13);
        tooltipDescriptionStyle.Italic = true;
        fonts[FontType.TooltipDescription] = Service.Interface.UiBuilder.FontAtlas.NewGameFontHandle(tooltipDescriptionStyle);
        fonts[FontType.ClearedOnHeader] = this.From(GameFontFamilyAndSize.Jupiter16);

        fonts[FontType.ProgressFont] = Service.Interface.UiBuilder.FontAtlas.NewGameFontHandle(new GameFontStyle(GameFontFamily.TrumpGothic, 18));

        this.fonts = fonts.AsReadOnly();
    }

    private IFontHandle From(GameFontFamilyAndSize familyAndSize)
    {
        return Service.Interface.UiBuilder.FontAtlas.NewGameFontHandle(new GameFontStyle(familyAndSize));
    }

    private IFontHandle FontHandle(FontType fontType)
    {
        return this.useDefaultFonts.Invoke()
            ? Service.Interface.UiBuilder.DefaultFontHandle
            : this.fonts[fontType];
    }

    private enum FontType
    {
        Default,
        DefaultSmaller,
        DefaultSmallerStraight,
        EncounterTypeHeader,
        TooltipDescription,
        ClearedOnHeader,
        ProgressFont,
    }

    public void Dispose()
    {
        this.fonts.Values.ToList().ForEach(font => font.Dispose());
    }
}
