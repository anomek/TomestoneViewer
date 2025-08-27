using Dalamud.Bindings.ImGui;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Interface.Textures;
using TomestoneViewer.GUI;

namespace TomestoneViewer.Character;

public record JobId(uint Id)
{
    public static readonly JobId Empty = new(0);

    public Icon Icon => new(Service.TextureProvider.GetFromGameIcon(new GameIconLookup(this == Empty ? 62143 : this.Id + 62100)).GetWrapOrEmpty().Handle, 25);

    public Icon SmallIcon => new(Service.TextureProvider.GetFromGameIcon(new GameIconLookup(this == Empty ? 62143 : this.Id + 62225)).GetWrapOrEmpty().Handle, 20);

    public override string ToString()
    {
        return this.Id.ToString();
    }
}
