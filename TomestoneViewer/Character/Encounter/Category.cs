using System;
using System.Collections.Generic;
using System.Linq;

namespace TomestoneViewer.Character.Encounter;

public record Category(
    string DisplayName,
    string CategoryQueryParam,
    string ZoneQueryParam,

    // We need release date only for savage (since we don't have accurate clear times for ultimates anyway)
    // So this is bound to category (because all savages will be in same category)
    DateTime? ReleaseDate)
{
    public static readonly Category ULTIMATE = new("Ultimates", "5", "ultimates", null);
    public static readonly Category SAVAGE = new("Savage", "3", "anabaseios-savage", new(2023, 5, 30));

    private static readonly IReadOnlyDictionary<Category, IReadOnlyList<Location>> CategoryLocations;

    public IReadOnlyList<Location> Locations => CategoryLocations[this];

    static Category()
    {
        CategoryLocations = Location.All()
           .GroupBy(location => location.Category)
           .ToDictionary(group => group.Key, group => (IReadOnlyList<Location>)group.ToList().AsReadOnly())
           .AsReadOnly();
    }

    public static IEnumerable<Category> All()
    {
        return CategoryLocations.Keys;
    }
}
