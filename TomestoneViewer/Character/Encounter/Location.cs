using System;
using System.Collections.Generic;
using System.Linq;

namespace TomestoneViewer.Character.Encounter;

public record Location(
    string DisplayName,
    Category Category,
    TerritoryId TerritoryId,
    TomestoneLocation Tomestone,
    FFLogsLocation FFLogs)
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1117:Parameters should be on same line or separate lines", Justification = "Need more flexible formatting here")]
    private static readonly IReadOnlyList<Location> ALL =
    [
        new("FRU", Category.ULTIMATE, new(1238),
            new(TomestoneLocation.TomestoneCategory.ULTIMATE, "futures-rewritten-ultimate",  ExpansionQueryParam.DT,  new(5651)),
            new(new FFLogsLocation.FFLogsZone(FFLogsZoneId.FRU, 1079, false))),
        new("TOP", Category.ULTIMATE, new(1122),
            new(TomestoneLocation.TomestoneCategory.ULTIMATE, "the-omega-protocol-ultimate", ExpansionQueryParam.EW, new(4652)),
            new(new(FFLogsZoneId.DT_LEGACY, 1077, false), new(FFLogsZoneId.TOP, 1068, true))),
        new("DSR", Category.ULTIMATE, new(968),
            new(TomestoneLocation.TomestoneCategory.ULTIMATE, "dragonsongs-reprise-ultimate", ExpansionQueryParam.EW, new(4651)),
            new(new(FFLogsZoneId.DT_LEGACY, 1076, false), new(FFLogsZoneId.DSR, 1065, true))),
        new("TEA", Category.ULTIMATE, new(887),
            new(TomestoneLocation.TomestoneCategory.ULTIMATE, "the-epic-of-alexander-ultimate", ExpansionQueryParam.SHB, new(3651)),
            new(new(FFLogsZoneId.DT_LEGACY, 1075, false), new(FFLogsZoneId.EW_LEGACY, 1062, true), new(FFLogsZoneId.TEA, 1050, true))),
        new("UWU",  Category.ULTIMATE, new(777),
            new(TomestoneLocation.TomestoneCategory.ULTIMATE, "the-weapons-refrain-ultimate",ExpansionQueryParam.STB,   new(2652)),
            new(new(FFLogsZoneId.DT_LEGACY, 1074, false), new(FFLogsZoneId.EW_LEGACY, 1061, true), new(FFLogsZoneId.SHB_LEGACY, 1048, true), new(FFLogsZoneId.UWU, 1042, true))),
        new("UCOB", Category.ULTIMATE, new(733),
            new(TomestoneLocation.TomestoneCategory.ULTIMATE, "the-unending-coil-of-bahamut-ultimate", ExpansionQueryParam.STB,  new(2651)),
            new(new(FFLogsZoneId.DT_LEGACY, 1073, false), new(FFLogsZoneId.EW_LEGACY, 1060, true), new(FFLogsZoneId.SHB_LEGACY, 1047, true), new(FFLogsZoneId.UCOB, 1039, true))),
        new("m5s", Category.SAVAGE, new(1257),
            new(TomestoneLocation.TomestoneCategory.SAVAGE, "dancing-green", ExpansionQueryParam.DT,  null),
            new(new FFLogsLocation.FFLogsZone(FFLogsZoneId.SAVAGE, 97, false))),
        new("m6s", Category.SAVAGE, new(1259),
            new(TomestoneLocation.TomestoneCategory.SAVAGE, "sugar-riot", ExpansionQueryParam.DT,  null),
            new(new FFLogsLocation.FFLogsZone(FFLogsZoneId.SAVAGE, 98, false))),
        new("m7s", Category.SAVAGE, new(1261),
            new(TomestoneLocation.TomestoneCategory.SAVAGE, "brute-abombinator", ExpansionQueryParam.DT, null),
            new(new FFLogsLocation.FFLogsZone(FFLogsZoneId.SAVAGE, 99, false))),
        new("m8s", Category.SAVAGE, new(1263),
            new(TomestoneLocation.TomestoneCategory.SAVAGE, "howling-blade", ExpansionQueryParam.DT, null),
            new(new FFLogsLocation.FFLogsZone(FFLogsZoneId.SAVAGE, 100, false))),
    ];

    public static IReadOnlyList<Location> All()
    {
        return ALL;
    }

    public static Location? Find(TerritoryId territoryId)
    {
        return All()
            .Where(location => location.TerritoryId == territoryId)
            .FirstOrDefault();
    }

    internal static IReadOnlySet<TerritoryId> AllTerritories()
    {
        return All()
            .Select(location => location.TerritoryId)
            .ToHashSet();
    }
}
