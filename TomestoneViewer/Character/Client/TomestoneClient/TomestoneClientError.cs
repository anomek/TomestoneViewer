using System.Numerics;

using Dalamud.Interface;
using Dalamud.Interface.Colors;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.Client.TomestoneClient;

public record TomestoneClientError(string Message, IRenderableError.ErrorType Type, bool Cachable) : IClientError
{
    /// <summary>
    /// seperate property to know if we can ignore error returned by FetchSummary
    /// in practice this is === !cachable, but it's seperate property in case logic changes in future.
    /// </summary>
    public bool CanIgnore => !this.Cachable;

    public static readonly TomestoneClientError CharacterDoesNotExist = new(
        "Character does not exist", IRenderableError.ErrorType.ERROR, true);

    public static readonly TomestoneClientError CharacterTomestoneDisabled = new(
        "Character Tomestone is disabled", IRenderableError.ErrorType.FEATURE_DISABLED, true);

    public static readonly TomestoneClientError CharacterActivityStreamDisabled = new(
        "Character log activity disabled (on Tomestone or FFLogs)", IRenderableError.ErrorType.FEATURE_DISABLED, true);

    public static readonly TomestoneClientError InertiaVersionNotFound = new(
        "Tomestone client error (intertia version not found)", IRenderableError.ErrorType.ERROR, false);

    public static readonly TomestoneClientError ServerResponseError = new(
        "Tomestone client error (unexpected response from server)", IRenderableError.ErrorType.ERROR, false);

    public static readonly TomestoneClientError NetworkError = new(
        "Tomestone client error (unknown network error)", IRenderableError.ErrorType.ERROR, false);

    public static readonly TomestoneClientError InternalError = new(
        "Tomestone client error (unknown internal error)", IRenderableError.ErrorType.ERROR, false);

    public static readonly TomestoneClientError RequestCancelled = new(
        "Requst to server canceled (you should never see this)", IRenderableError.ErrorType.ERROR, false);
}
