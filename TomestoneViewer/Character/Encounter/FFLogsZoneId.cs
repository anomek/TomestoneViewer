using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomestoneViewer.Character.Encounter;
public record FFLogsZoneId(uint zoneId)
{
    public static readonly FFLogsZoneId SAVAGE = new(68);
    public static readonly FFLogsZoneId FRU = new(65);
    public static readonly FFLogsZoneId EW_LEGACY = new(59);

    public override string ToString()
    {
        return this.zoneId.ToString();
    }
}
