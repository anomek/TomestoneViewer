using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;

namespace TomestoneViewer.Character;

internal record CharacterSelectorError(string Message, Vector4 Color, FontAwesomeIcon Symbol) : IRenderableError
{
    public bool IsClickable => false;

    public static readonly CharacterSelectorError InvalidTarget = new(
        "Invalid target", ImGuiColors.DalamudWhite, FontAwesomeIcon.ExclamationCircle);

    public static readonly CharacterSelectorError ClipboardError = new(
        "Clipboard error", ImGuiColors.DalamudRed, FontAwesomeIcon.ExclamationCircle);

    public static readonly CharacterSelectorError Unimplemented = new(
        "Feature is not yet implemented", ImGuiColors.DalamudWhite, FontAwesomeIcon.ExclamationCircle);

    public static readonly CharacterSelectorError EmptyHomeWorld = new(
        "Cannot find character's home world", ImGuiColors.DalamudRed, FontAwesomeIcon.ExclamationCircle);

    public static readonly CharacterSelectorError NoCharacterSelected = new(
        "No character is selected", ImGuiColors.DalamudWhite, FontAwesomeIcon.ExclamationCircle);
}
