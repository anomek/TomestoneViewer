using System;
using System.Numerics;

using Dalamud.Interface.Internal;
using ImGuiNET;

namespace TomestoneViewer.GUI;

public class Icon(IDalamudTextureWrap handle, Vector2 size, Vector2 upperLeftCorner) : IDisposable
{
    private readonly IDalamudTextureWrap handle = handle;
    private readonly Vector2 size = size;
    private readonly Vector2 uv0 = upperLeftCorner / handle.Size;
    private readonly Vector2 uv1 = (upperLeftCorner + size) / handle.Size;

    public Vector2 Size => this.size;

    public void ImGuiImage()
    {
        this.ImGuiImage(this.size);
    }

    public void ImGuiImage(Vector2 size)
    {
        ImGui.Image(this.handle.ImGuiHandle, size, this.uv0, this.uv1);
    }

    public void Dispose()
    {
        this.handle.Dispose();
    }
}
