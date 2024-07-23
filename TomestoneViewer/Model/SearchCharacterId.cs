using TomestoneViewer.Character;

namespace TomestoneViewer.Model;

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Need public fields for UI code to work properly")]
public class SearchCharacterId
{
    public string FirstName = string.Empty;
    public string LastName = string.Empty;
    public string World = string.Empty;

    public void Copy(CharacterId characterId)
    {
        this.FirstName = characterId.FirstName;
        this.LastName = characterId.LastName;
        this.World = characterId.World;
    }

    public CharacterId ToCharacterId()
    {
        return new CharacterId(this.FirstName, this.LastName, this.World);
    }

    public void Reset()
    {
        this.FirstName = string.Empty;
        this.LastName = string.Empty;
        this.World = string.Empty;
    }
}
