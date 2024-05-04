using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;

namespace TomestoneViewer.Character;

internal record CharacterSelectorError(string Message, Vector4 Color, FontAwesomeIcon Symbol) : IRenderableError
{

    private readonly string message = Message;
    private readonly Vector4 color = Color;
    private readonly FontAwesomeIcon symbol = Symbol;

    public string Message => this.message;

    public Vector4 Color => this.color;

    public FontAwesomeIcon Symbol => this.symbol;

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
