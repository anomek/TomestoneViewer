using System;

namespace TomestoneViewer.Character;

public record CharacterId(string FirstName, string LastName, string World)
{
    public string FullName => $"{this.FirstName} {this.LastName}";

    public static CharacterId? FromFullName(string fullName, string world)
    {
        var split = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (split.Length != 2)
        {
            return null;
        }

        return new CharacterId(split[0], split[1], world);
    }

    public static CharacterId? FromQualifiedName(string qualifiedName)
    {
        var split = qualifiedName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (split.Length != 3)
        {
            return null;
        }

        return new CharacterId(split[0], split[1], split[2]);
    }

    public override string ToString()
    {
        return $"{this.FullName}@{this.World}";
    }
}
