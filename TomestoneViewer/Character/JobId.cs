using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;

namespace TomestoneViewer.Character;

public record JobId(uint Id)
{
    public static readonly JobId Empty = new(0);

    public ImTextureID ImGuiIconHandle => Service.TextureProvider.GetFromGameIcon(new GameIconLookup(this.Id + 62100)).GetWrapOrEmpty().Handle;

    public override string ToString()
    {
        return this.Id.ToString();
    }
}
