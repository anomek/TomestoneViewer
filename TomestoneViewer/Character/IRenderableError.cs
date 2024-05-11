using System.Numerics;

using Dalamud.Interface;

namespace TomestoneViewer.Character;

public interface IRenderableError
{
    string Message { get; }

    Vector4 Color { get; }

    FontAwesomeIcon Symbol { get; }
}
