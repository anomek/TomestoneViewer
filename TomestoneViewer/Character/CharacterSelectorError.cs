using System.Numerics;

using Dalamud.Interface;
using Dalamud.Interface.Colors;

namespace TomestoneViewer.Character;

internal record CharacterSelectorError(string Message) : IRenderableError
{
    public Vector4 Color => ImGuiColors.DalamudWhite;

    public FontAwesomeIcon Symbol => FontAwesomeIcon.ExclamationCircle;

    public static readonly CharacterSelectorError InvalidTarget = new("Invalid target");
    public static readonly CharacterSelectorError ClipboardError = new("Clipboard error");
    public static readonly CharacterSelectorError Unimplemented = new("Feature is not yet implemented");
    public static readonly CharacterSelectorError EmptyHomeWorld = new("Cannot find character's home world");
    public static readonly CharacterSelectorError NoCharacterSelected = new("No character is selected");
    public static readonly CharacterSelectorError InvalidCharacterName = new("Invalid character name");
}
