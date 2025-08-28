using TomestoneViewer.Character.Client.FFLogsClient;
using TomestoneViewer.Character.Client.TomestoneClient;

namespace TomestoneViewer.Character;

public class CharDataFactory(ITomestoneClient tomestoneClient, IFFLogsClient fflogsClient)
{
    private readonly ITomestoneClient client = tomestoneClient;
    private readonly IFFLogsClient fflogsClient = fflogsClient;

    public CharData Create(CharacterId characterId)
    {
        return new CharData(characterId, new CharDataLoader(characterId, this.client, this.fflogsClient));
    }
}
