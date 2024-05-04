using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.TomestoneClient;


public record TomestoneClientError(string Message, Vector4 Color, FontAwesomeIcon Symbol, bool Cachable) : IEncounterDataError
{

    private readonly string message = Message;
    private readonly Vector4 color = Color;
    private readonly FontAwesomeIcon symbol = Symbol;

    /// <summary>
    /// Responses with this error can be cached
    /// </summary>
    private readonly bool cachable = Cachable;

    public bool Cachable { get => this.cachable; }

    /// <summary>
    /// seperate property to know if we can ignore error returned by FetchSummary
    /// in practice this is === !cachable, but it's seperate property in case logic changes in future
    /// </summary>
    public bool CanIgnore { get => !this.cachable; }

    public bool IsClickable { get => false; }

    public string Message { get => this.message; }

    public Vector4 Color { get => this.color; }

    public FontAwesomeIcon Symbol { get => this.symbol; }


    public static readonly TomestoneClientError CharacterDoesNotExist = new(
        "Character does not exist", ImGuiColors.DalamudRed, FontAwesomeIcon.ExclamationCircle, true);

    public static readonly TomestoneClientError CharacterTomestoneDisabled = new(
        "Character Tomestone is disabled", new Vector4(0.1f, 0.6f, 0.9f, 1), FontAwesomeIcon.QuestionCircle, true);

    public static readonly TomestoneClientError CharacterActivityStreamDisabled = new(
        "Character log activity disabled (on Tomestone or FFLogs)", new Vector4(0.1f, 0.6f, 0.9f, 1), FontAwesomeIcon.QuestionCircle, true);

    public static readonly TomestoneClientError InertiaVersionNotFound = new(
        "Tomestone client error (intertia version not found)", ImGuiColors.DalamudRed, FontAwesomeIcon.ExclamationCircle, false);

    public static readonly TomestoneClientError ServerResponseError = new(
        "Tomestone client error (unexpected response from server)", ImGuiColors.DalamudRed, FontAwesomeIcon.ExclamationCircle, false);

    public static readonly TomestoneClientError NetworkError = new(
        "Tomestone client error (unknown network error)", ImGuiColors.DalamudRed, FontAwesomeIcon.ExclamationCircle, false);

    public static readonly TomestoneClientError InternalError = new(
        "Tomestone client error (unknown internal error)", ImGuiColors.DalamudRed, FontAwesomeIcon.ExclamationCircle, false);

    public static readonly TomestoneClientError RequestCancelled = new(
        "Requst to server canceled (you should never see this)", ImGuiColors.DalamudRed, FontAwesomeIcon.ExclamationCircle, false);
}
