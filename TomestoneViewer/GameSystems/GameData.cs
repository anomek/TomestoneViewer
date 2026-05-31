using System;
using System.Collections.Concurrent;
using System.Linq;

using Lumina.Excel.Sheets;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.GameSystems;

public class GameData
{
    private readonly ConcurrentDictionary<string, TerritoryId?> territoryCache = [];
    private readonly ConcurrentDictionary<string, string?> regionCache = [];
    private readonly ConcurrentDictionary<ushort, string?> worldNameCache = [];
    private readonly ConcurrentDictionary<string, uint?> worldIdChache = [];

    public TerritoryId? GetTerritoryId(string dutyName)
    {
        dutyName = dutyName.ToLower();
        return this.territoryCache.GetOrAdd(dutyName, this.FindTerritoryId);
    }

    public string? GetRegion(string worldName)
    {
        worldName = worldName.ToLower();
        return this.regionCache.GetOrAdd(worldName, this.FindRegion);
    }

    public string? GetWorldName(ushort worldId)
    {
        return this.worldNameCache.GetOrAdd(worldId, this.FindWorldName);
    }

    internal uint? GetWorldId(string worldName)
    {
        return this.worldIdChache.GetOrAdd(worldName, this.FindWorldId);
    }

    private TerritoryId? FindTerritoryId(string dutyName)
    {
        var entry = Service.DataManager.GetExcelSheet<ContentFinderCondition>()?
            .Where(entry => string.Equals(
                entry.Name.ToString(),
                dutyName,
                StringComparison.InvariantCultureIgnoreCase))
            .Select(entry => (ContentFinderCondition?)entry)
            .FirstOrDefault();
        return entry.HasValue ? new TerritoryId(entry.Value.TerritoryType.RowId) : null;
    }

    private string? FindRegion(string worldName)
    {
        var world = Service.DataManager.GetExcelSheet<World>()?
            .Where(entry => string.Equals(
                            entry.Name.ToString(),
                            worldName,
                            StringComparison.InvariantCultureIgnoreCase))
            .Select(entry => (World?)entry)
            .FirstOrDefault();
        if (world is not { IsPublic: true })
        {
            return null;
        }

        return world.Value.DataCenter.Value.Region.RowId switch
        {
            1 => "JP",
            2 => "NA",
            3 => "EU",
            4 => "OC",
            _ => null,
        };
    }

    private string? FindWorldName(ushort worldId)
    {
        var world = Service.DataManager.GetExcelSheet<World>()?
            .Where(x => x.RowId == worldId)
            .Select(x => (World?)x)
            .FirstOrDefault();
        if (world is not { IsPublic: true })
        {
            return null;
        }

        return world.Value.Name.ToString();
    }

    private uint? FindWorldId(string worldName)
    {
        var world = Service.DataManager.GetExcelSheet<World>()?
            .Where(x => x.Name.ToString() == worldName)
            .Select(x => (World?)x)
            .FirstOrDefault();
        if (world is not { IsPublic: true })
        {
            return null;
        }

        return world.Value.RowId;
    }
}
