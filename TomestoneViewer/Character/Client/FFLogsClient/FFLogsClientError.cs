using System.Numerics;

using Dalamud.Interface;
using TomestoneViewer.Character.Encounter.Data;

namespace TomestoneViewer.Character.Client.FFLogsClient;

public record FFLogsClientError(string Message, Vector4 Color, FontAwesomeIcon Symbol, bool Cachable) : IEncounterDataError, IClientError
{
}
