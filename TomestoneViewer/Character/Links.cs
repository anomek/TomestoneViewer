using Lumina;
using TomestoneViewer.Character.Encounter;
using TomestoneViewer.Character.TomestoneClient;

namespace TomestoneViewer.Character;

public class Links
{
    private readonly CharacterId characterId;
    private readonly CharDataLoader loader;

    internal Links(CharacterId characterId, CharDataLoader loader)
    {
        this.characterId = characterId;
        this.loader = loader;
    }

    public string TomestoneMain()
    {
        if (this.loader.LodestoneId != null)
        {
            return $"https://tomestone.gg/character/{this.loader.LodestoneId}/{this.characterId.FirstName.ToLower()}-{this.characterId.LastName.ToLower()}";
        }
        else
        {
            return $"https://tomestone.gg/character-name/{this.characterId.World}/{this.characterId.FullName}";
        }
    }

    public string EncounterActivity(Location location)
    {
        return this.TomestoneMain()
            + $"/activity?category={location.Category.CategoryQueryParam}&encounter={location.EncounterQueryParam}"
            + $"&expansion={location.ExpansionQueryParam}&zone={location.Category.ZoneQueryParam}";
    }

    public string? FFLogs()
    {
        var region = Service.GameData.GetRegion(this.characterId.World);
        if (region == null)
        {
            return null;
        }
        else
        {
            return $"https://www.fflogs.com/character/{region.ToLower()}/{this.characterId.World.ToLower()}/"
                + $"{this.characterId.FullName.ToLower()}";
        }
    }
}
