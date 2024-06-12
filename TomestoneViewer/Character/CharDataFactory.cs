using TomestoneViewer.Character.Client.TomestoneClient;

namespace TomestoneViewer.Character;

public class CharDataFactory(ITomestoneClient client)
{
    private readonly ITomestoneClient client = client;

    public CharData Create(CharacterId characterId)
    {
        return new CharData(characterId, new CharDataLoader(characterId, this.client));
    }
}
