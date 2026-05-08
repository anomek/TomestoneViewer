using System.Numerics;

namespace TomestoneViewer.GUI.Widgets;

public interface IWidget
{
    Vector2 Draw();

    float GetMinWidth();
}
