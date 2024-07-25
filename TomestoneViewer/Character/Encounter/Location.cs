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
        new("TOP", "the-omega-protocol-ultimate", Category.ULTIMATE, ExpansionQueryParam.EW, new(1122), new(4652)),
        new("DSR", "dragonsongs-reprise-ultimate", Category.ULTIMATE, ExpansionQueryParam.EW, new(968), new(4651)),
        new("TEA", "the-epic-of-alexander-ultimate", Category.ULTIMATE, ExpansionQueryParam.SHB, new(887),  new(3651)),
        new("UWU", "the-weapons-refrain-ultimate", Category.ULTIMATE, ExpansionQueryParam.STB, new(777),  new(2652)),
        new("UCOB", "the-unending-coil-of-bahamut-ultimate", Category.ULTIMATE, ExpansionQueryParam.STB, new(733), new(2651)),
        new("r1s",  "black-cat", Category.SAVAGE, ExpansionQueryParam.DT, new(1226), null),
        new("r2s", "honey-b.-lovely", Category.SAVAGE, ExpansionQueryParam.DT, new(1228), null),
        new("r3s",  "brute-bomber", Category.SAVAGE, ExpansionQueryParam.DT, new(1230), null),
        new("r4s", "wicked-thunder", Category.SAVAGE, ExpansionQueryParam.DT, new(1232), null),
        // new("r4sp2", "pallas-wicked-thunder", Category.SAVAGE, ExpansionQueryParam.DT, TerritoryId.Empty, null),
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
