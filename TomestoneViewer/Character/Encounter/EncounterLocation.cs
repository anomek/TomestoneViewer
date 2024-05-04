using System.Collections.Generic;

namespace TomestoneViewer.Character.Encounter;

// FIXME: redesign / cleanup
public record EncounterLocation
{
    private static readonly string EndwalkerExpansionQueryParam = "endwalker";
    private static readonly string ShadowbringersExpansionQueryParam = "shadowbringers";
    private static readonly string StormbloodExpansionQueryParam = "stormblood";

    public static readonly List<Category> LOCATIONS =
    [
        new()
        {
            DisplayName = "Ultimates",
            CategoryQueryParam = "5",
            ZoneQueryParam = "ultimates",
            Locations =
            [
                new()
                {
                    DisplayName = "TOP",
                    EncounterQueryParam = "the-omega-protocol-ultimate",
                    ExpansionQueryParam = EndwalkerExpansionQueryParam,
                    Id = new(4652),
                },
                new()
                {
                    DisplayName = "DSR",
                    EncounterQueryParam = "dragonsongs-reprise-ultimate",
                    ExpansionQueryParam = EndwalkerExpansionQueryParam,
                    Id = new(4651),
                },
                new()
                {
                    DisplayName = "TEA",
                    EncounterQueryParam = "the-epic-of-alexander-ultimate",
                    ExpansionQueryParam = ShadowbringersExpansionQueryParam,
                    Id = new(3651),
                },
                new()
                {
                    DisplayName = "UWU",
                    EncounterQueryParam = "the-weapons-refrain-ultimate",
                    ExpansionQueryParam = StormbloodExpansionQueryParam,
                    Id = new(2652),
                },
                new()
                {
                    DisplayName = "UCOB",
                    EncounterQueryParam = "the-unending-coil-of-bahamut-ultimate",
                    ExpansionQueryParam = StormbloodExpansionQueryParam,
                    Id = new(2651),
                },
            ],
        },
        new()
        {
            DisplayName = "Savage",
            CategoryQueryParam = "3",
            ZoneQueryParam = "anabaseios-savage",
            Locations =
            [
                new()
                {
                    DisplayName = "p9s",
                    EncounterQueryParam = "kokytos",
                    ExpansionQueryParam = EndwalkerExpansionQueryParam,
                },
                new()
                {
                    DisplayName = "p10s",
                    EncounterQueryParam = "pand%C3%A6monium",
                    ExpansionQueryParam = EndwalkerExpansionQueryParam,
                },
                new()
                {
                    DisplayName = "p11s",
                    EncounterQueryParam = "themis",
                    ExpansionQueryParam = EndwalkerExpansionQueryParam,
                },
                new()
                {
                    DisplayName = "p12sp1",
                    EncounterQueryParam = "athena",
                    ExpansionQueryParam = EndwalkerExpansionQueryParam,
                },
                new()
                {
                    DisplayName = "p12sp2",
                    EncounterQueryParam = "pallas-athena",
                    ExpansionQueryParam = EndwalkerExpansionQueryParam,
                },
            ],
        },
    ];

    public static List<EncounterLocation> AllLocations()
    {
        List<EncounterLocation> ret = [];
        foreach (var category in LOCATIONS)
        {
            foreach (var location in category.Locations)
            {
                ret.Add(location);
            }
        }
        return ret;
    }

    public string DisplayName { get; set; } = null!;
    public string EncounterQueryParam { get; set; } = null!;
    public string ExpansionQueryParam { get; set; } = null!;

    public EncounterId? Id { get; set; }

    public class Category
    {

        public string DisplayName { get; set; } = null!;
        public string CategoryQueryParam { get; set; } = null!;
        public string ZoneQueryParam { get; set; } = null!;

        public List<EncounterLocation> Locations { get; set; } = null!;
    }
}
