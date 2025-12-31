using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomestoneViewer.Character.Encounter;

public record FFLogsLocation(params IReadOnlyList<FFLogsLocation.FFLogsZone> Zones)
{
    public record FFLogsZone(
        int BossId,
        bool PreviousExpansion)
    {
    }
}
