using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.Client;

public interface IClientError : IEncounterDataError
{
    bool Cachable { get; }
}
