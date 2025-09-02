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
    public static readonly FFLogsZoneId TOP = new(53);
    public static readonly FFLogsZoneId DSR = new(45);
    public static readonly FFLogsZoneId TEA = new(32);
    public static readonly FFLogsZoneId UWU = new(23);
    public static readonly FFLogsZoneId UCOB = new(19);

    public static readonly FFLogsZoneId DT_LEGACY = new(59);
    public static readonly FFLogsZoneId EW_LEGACY = new(43);
    public static readonly FFLogsZoneId SHB_LEGACY = new(30);

    public override string ToString()
    {
        return this.zoneId.ToString();
    }
}
