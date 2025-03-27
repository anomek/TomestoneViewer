using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public static readonly Category ULTIMATE = new("Ultimates", "ultimates", "ultimates", null);
    public static readonly Category SAVAGE = new("Savage", "raids", "aac-cruiserweight-savage", new(2025, 4, 1));

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:Field names should begin with lower-case letter", Justification = "I have to break at least one rule")]
    private static IReadOnlyDictionary<Category, IReadOnlyList<Location>>? CategoryLocations;

    public IReadOnlyList<Location> Locations => CategoryLocationsDict()[this];

    private static ReadOnlyDictionary<Category, IReadOnlyList<Location>> LoadCategoriesDict()
    {
        return Location.All()
           .GroupBy(location => location.Category)
           .ToDictionary(group => group.Key, group => (IReadOnlyList<Location>)group.ToList().AsReadOnly())
           .AsReadOnly();
    }

    private static IReadOnlyDictionary<Category, IReadOnlyList<Location>> CategoryLocationsDict()
    {
        return CategoryLocations ??= LoadCategoriesDict();
    }

    public static IEnumerable<Category> All()
    {
        return CategoryLocationsDict().Keys;
    }
}
