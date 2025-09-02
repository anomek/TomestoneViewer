using Dalamud.Interface;
using Dalamud.Interface.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TomestoneViewer.Character.Client.TomestoneClient;

namespace TomestoneViewer.Character.Client.FFLogsClient;
public record FFLogsClientError(string Message, IRenderableError.ErrorType Type, bool Cachable) : IClientError
{
    public static readonly FFLogsClientError SignatureNotFound = new(
        "FFLogs client error (signature not found)", IRenderableError.ErrorType.ERROR, false);

    public static readonly FFLogsClientError ServerResponseError = new(
        "FFLogs client error (unexpected response from server)", IRenderableError.ErrorType.ERROR, false);

    public static readonly FFLogsClientError RequestCancelled = new(
        "Requst to server canceled (you should never see this)", IRenderableError.ErrorType.ERROR, false);

    public static readonly FFLogsClientError TooManyRequestsError = new(
        "FFLogs client error (too many requests)", IRenderableError.ErrorType.ERROR, false);

    public static readonly FFLogsClientError ContentExpired = new(
        "FFLogs client error (content expired)", IRenderableError.ErrorType.ERROR, false);

    public static readonly FFLogsClientError InternalError = new(
        "FFLogs client error (unknown internal error)", IRenderableError.ErrorType.ERROR, false);
}
