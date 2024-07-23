using System;
using System.Collections.Concurrent;
using System.Linq;

using Lumina.Excel.GeneratedSheets;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.GameSystems;

public class GameData
{
    private readonly ConcurrentDictionary<string, TerritoryId?> territoryCache = [];
    private readonly ConcurrentDictionary<string, string?> regionCache = [];
    private readonly ConcurrentDictionary<ushort, string?> worldNameCache = [];

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

    private TerritoryId? FindTerritoryId(string dutyName)
    {
        var id = Service.DataManager.GetExcelSheet<ContentFinderCondition>()!
                  .FirstOrDefault(entry => string.Equals(
                         entry.Name.ToString(),
                         dutyName,
                         StringComparison.InvariantCultureIgnoreCase))?
                   .TerritoryType.Row;
        return id == null ? null : new(id.Value);
    }

    private string? FindRegion(string worldName)
    {
        var world = Service.DataManager.GetExcelSheet<World>()?
            .FirstOrDefault(entry => string.Equals(
                            entry.Name.ToString(),
                            worldName,
                            StringComparison.InvariantCultureIgnoreCase));
        if (world is not { IsPublic: true })
        {
            return null;
        }

        return world.DataCenter?.Value?.Region switch
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
        var world = Service.DataManager.GetExcelSheet<World>()?.FirstOrDefault(x => x.RowId == worldId);
        if (world is not { IsPublic: true })
        {
            return null;
        }

        return world.Name;
    }
}
