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
public record FFLogsClientError(string Message, Vector4 Color, FontAwesomeIcon Symbol, bool Cachable) : IClientError
{
    public static readonly FFLogsClientError SignatureNotFound = new(
        "FFLogs client error (signature version not found)", ImGuiColors.DalamudRed, FontAwesomeIcon.ExclamationCircle, false);

    public static readonly FFLogsClientError ServerResponseError = new(
        "FFLogs client error (unexpected response from server)", ImGuiColors.DalamudRed, FontAwesomeIcon.ExclamationCircle, false);
}
