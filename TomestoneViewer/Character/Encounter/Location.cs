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
            new(new FFLogsLocation.FFLogsZone(1079, false))),
        new("TOP", Category.ULTIMATE, new(1122),
            new(TomestoneLocation.TomestoneCategory.ULTIMATE, "the-omega-protocol-ultimate", ExpansionQueryParam.EW, new(4652)),
            new(new(1077, false), new(1068, true))),
        new("DSR", Category.ULTIMATE, new(968),
            new(TomestoneLocation.TomestoneCategory.ULTIMATE, "dragonsongs-reprise-ultimate", ExpansionQueryParam.EW, new(4651)),
            new(new(1076, false), new(1065, true))),
        new("TEA", Category.ULTIMATE, new(887),
            new(TomestoneLocation.TomestoneCategory.ULTIMATE, "the-epic-of-alexander-ultimate", ExpansionQueryParam.SHB, new(3651)),
            new(new(1075, false), new(1062, true), new(1050, true))),
        new("UWU",  Category.ULTIMATE, new(777),
            new(TomestoneLocation.TomestoneCategory.ULTIMATE, "the-weapons-refrain-ultimate",ExpansionQueryParam.STB,   new(2652)),
            new(new(1074, false), new(1061, true), new(1048, true), new(1042, true))),
        new("UCOB", Category.ULTIMATE, new(733),
            new(TomestoneLocation.TomestoneCategory.ULTIMATE, "the-unending-coil-of-bahamut-ultimate", ExpansionQueryParam.STB,  new(2651)),
            new(new(1073, false), new(1060, true), new(1047, true), new(1039, true))),
        new("m9s", Category.SAVAGE, new(1321),
            new(TomestoneLocation.TomestoneCategory.SAVAGE, "vamp-fatale", ExpansionQueryParam.DT,  null),
            new(new FFLogsLocation.FFLogsZone(101, false))),
        new("m10s", Category.SAVAGE, new(1323),
            new(TomestoneLocation.TomestoneCategory.SAVAGE, "red-hot-deep-blue", ExpansionQueryParam.DT,  null),
            new(new FFLogsLocation.FFLogsZone(102, false))),
        new("m11s", Category.SAVAGE, new(1325),
            new(TomestoneLocation.TomestoneCategory.SAVAGE, "the-tyrant", ExpansionQueryParam.DT, null),
            new(new FFLogsLocation.FFLogsZone(103, false))),
        new("m12s", Category.SAVAGE, new(1327),
            new(TomestoneLocation.TomestoneCategory.SAVAGE, "lindwurm", ExpansionQueryParam.DT, null),
            new(new FFLogsLocation.FFLogsZone(104, false))),
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
