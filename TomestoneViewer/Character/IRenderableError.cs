using Dalamud.Interface;
using Dalamud.Interface.Colors;
using System.Numerics;
using System.Reflection;

namespace TomestoneViewer.Character;

public interface IRenderableError
{
    string Message { get; }

    ErrorType Type { get; }

    public record ErrorType(Vector4 Color, FontAwesomeIcon Symbol, string Glyph)
    {
        public static readonly ErrorType ERROR = new(ImGuiColors.DalamudRed, FontAwesomeIcon.ExclamationCircle, "!!");
        public static readonly ErrorType WARN = new(ImGuiColors.DalamudOrange, FontAwesomeIcon.QuestionCircle, "??");
        public static readonly ErrorType FEATURE_DISABLED = new(new Vector4(0.1f, 0.6f, 0.9f, 1), FontAwesomeIcon.QuestionCircle, "??");
        public static readonly ErrorType INFO = new(ImGuiColors.DalamudWhite, FontAwesomeIcon.ExclamationCircle, "!!");
    }
}
