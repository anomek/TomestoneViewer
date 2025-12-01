using TomestoneViewer.Character.Client.FFLogsClient;
using TomestoneViewer.Character.Client.TomestoneClient;
using TomestoneViewer.External;

namespace TomestoneViewer.Character;

public class CharDataFactory(ITomestoneClient tomestoneClient, IFFLogsClient fflogsClient, PlayerTrackInterface playerTrackInterface)
{
    private readonly ITomestoneClient client = tomestoneClient;
    private readonly IFFLogsClient fflogsClient = fflogsClient;
    private readonly PlayerTrackInterface playerTrackInterface = playerTrackInterface;

    public CharData Create(CharacterId characterId)
    {
        return new CharData(characterId, new CharDataLoader(characterId, this.client, this.fflogsClient, this.playerTrackInterface));
    }
}
