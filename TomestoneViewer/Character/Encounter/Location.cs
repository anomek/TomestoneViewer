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
        new("p9s",  "kokytos", Category.SAVAGE, ExpansionQueryParam.EW, new(1148), null),
        new("p10s", "pand%C3%A6monium", Category.SAVAGE, ExpansionQueryParam.EW, new(1150), null),
        new("p11s",  "themis", Category.SAVAGE, ExpansionQueryParam.EW, new(1152), null),
        new("p12sp1", "athena", Category.SAVAGE, ExpansionQueryParam.EW, new(1154), null),
        new("p12sp2", "pallas-athena", Category.SAVAGE, ExpansionQueryParam.EW, TerritoryId.Empty, null)
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
