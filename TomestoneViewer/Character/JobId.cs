using Dalamud.Interface.Textures;

namespace TomestoneViewer.Character;

public record JobId(uint Id)
{
    public static readonly JobId Empty = new(0);

    public nint ImGuiIconHandle => Service.TextureProvider.GetFromGameIcon(new GameIconLookup(this.Id + 62100)).GetWrapOrEmpty().ImGuiHandle;

    public override string ToString()
    {
        return this.Id.ToString();
    }
}
