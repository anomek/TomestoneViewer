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
            new(FFLogsZoneId.FRU, 1079)),
        new("TOP", Category.ULTIMATE, new(1122),
            new(TomestoneLocation.TomestoneCategory.ULTIMATE, "the-omega-protocol-ultimate", ExpansionQueryParam.EW, new(4652)),
            new(FFLogsZoneId.EW_LEGACY, 1077)),
        new("DSR", Category.ULTIMATE, new(968),
            new(TomestoneLocation.TomestoneCategory.ULTIMATE, "dragonsongs-reprise-ultimate", ExpansionQueryParam.EW, new(4651)),
            new(FFLogsZoneId.EW_LEGACY, 1076)),
        new("TEA", Category.ULTIMATE, new(887),
            new(TomestoneLocation.TomestoneCategory.ULTIMATE, "the-epic-of-alexander-ultimate", ExpansionQueryParam.SHB, new(3651)),
            new(FFLogsZoneId.EW_LEGACY, 1075)),
        new("UWU",  Category.ULTIMATE, new(777),
            new(TomestoneLocation.TomestoneCategory.ULTIMATE, "the-weapons-refrain-ultimate",ExpansionQueryParam.STB,   new(2652)),
            new(FFLogsZoneId.EW_LEGACY, 1074)),
        new("UCOB", Category.ULTIMATE, new(733),
            new(TomestoneLocation.TomestoneCategory.ULTIMATE, "the-unending-coil-of-bahamut-ultimate", ExpansionQueryParam.STB,  new(2651)),
            new(FFLogsZoneId.EW_LEGACY, 1073)),
        new("m5s", Category.SAVAGE, new(1257),
            new(TomestoneLocation.TomestoneCategory.SAVAGE, "dancing-green", ExpansionQueryParam.DT,  null),
            new(FFLogsZoneId.SAVAGE, 97)),
        new("m6s", Category.SAVAGE, new(1259),
            new(TomestoneLocation.TomestoneCategory.SAVAGE, "sugar-riot", ExpansionQueryParam.DT,  null),
            new(FFLogsZoneId.SAVAGE, 98)),
        new("m7s", Category.SAVAGE, new(1261),
            new(TomestoneLocation.TomestoneCategory.SAVAGE, "brute-abombinator", ExpansionQueryParam.DT, null),
            new(FFLogsZoneId.SAVAGE, 99)),
        new("m8s", Category.SAVAGE, new(1263),
            new(TomestoneLocation.TomestoneCategory.SAVAGE, "howling-blade", ExpansionQueryParam.DT, null),
            new(FFLogsZoneId.SAVAGE, 100)),
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
