using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Plugin.Services;
using TomestoneViewer.Character;

namespace TomestoneViewer.GUI;

public class Icon
{
    public ImTextureID Texture { get; private init; }

    public float Size { get; private init; }

    public Icon(ImTextureID texture, uint baseSize)
    {
        this.Texture = texture;
        this.Size = (float)Math.Round(ImGuiHelpers.GlobalScale * baseSize);
    }

    public void Draw()
    {
        ImGui.Image(this.Texture, new System.Numerics.Vector2(this.Size));
    }
}
