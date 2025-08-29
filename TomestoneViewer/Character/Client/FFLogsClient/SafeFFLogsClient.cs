using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomestoneViewer.Character.Client.FFLogsClient;
internal class SafeFFLogsClient(IFFLogsClient client) : IFFLogsClient
{
    private readonly IFFLogsClient client = client;

    public async Task Fetch(CharacterId characterId)
    {
        await this.client.Fetch(characterId);
    }
}
