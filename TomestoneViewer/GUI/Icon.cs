using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Plugin.Services;
using TomestoneViewer.Character;

namespace TomestoneViewer.GUI;

public class Icon
{
    public ImTextureID Texture { get; private init; }

    public float Size { get; private init; }

    private readonly float uv0;
    private readonly float uv1;

    public Icon(ImTextureID texture, uint baseSize)
        : this(texture, baseSize, 0, 1)
    {
    }

    public Icon(ImTextureID texture, float baseSize, float uv0, float uv1)
    {
        this.Texture = texture;
        this.Size = (float)Math.Round(ImGuiHelpers.GlobalScale * baseSize);
        this.uv0 = uv0;
        this.uv1 = uv1;
    }

    public void Draw()
    {
        ImGui.Image(this.Texture, new Vector2(this.Size), new Vector2(this.uv0), new Vector2(this.uv1));
    }
}
