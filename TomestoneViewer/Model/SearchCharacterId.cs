using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomestoneViewer.Character;

namespace TomestoneViewer.Model;

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
