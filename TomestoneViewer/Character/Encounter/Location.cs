using System;
using System.Collections.Generic;
using System.Linq;

namespace TomestoneViewer.Character.Encounter;

public record Location(
    string DisplayName,
    string EncounterQueryParam,
    Category Category,
    ExpansionQueryParam ExpansionQueryParam,
    TerritoryId TerritoryId,
    UltimateId? UltimateId)
{
    private static readonly IReadOnlyList<Location> ALL =
    [
        new("FRU", "futures-rewritten-ultimate", Category.ULTIMATE, ExpansionQueryParam.DT, new(1238), new(5651)),
        new("TOP", "the-omega-protocol-ultimate", Category.ULTIMATE, ExpansionQueryParam.EW, new(1122), new(4652)),
        new("DSR", "dragonsongs-reprise-ultimate", Category.ULTIMATE, ExpansionQueryParam.EW, new(968), new(4651)),
        new("TEA", "the-epic-of-alexander-ultimate", Category.ULTIMATE, ExpansionQueryParam.SHB, new(887),  new(3651)),
        new("UWU", "the-weapons-refrain-ultimate", Category.ULTIMATE, ExpansionQueryParam.STB, new(777),  new(2652)),
        new("UCOB", "the-unending-coil-of-bahamut-ultimate", Category.ULTIMATE, ExpansionQueryParam.STB, new(733), new(2651)),
        new("m5s", "dancing-green", Category.SAVAGE, ExpansionQueryParam.DT, new(1257), null),
        new("m6s", "sugar-riot", Category.SAVAGE, ExpansionQueryParam.DT, new(1259), null),
        new("m7s", "brute-abombinator", Category.SAVAGE, ExpansionQueryParam.DT, new(1261), null),
        new("m8s", "howling-blade", Category.SAVAGE, ExpansionQueryParam.DT, new(1263), null),
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
