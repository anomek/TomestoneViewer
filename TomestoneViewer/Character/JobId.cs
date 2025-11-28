using Dalamud.Bindings.ImGui;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Interface.Textures;
using System;
using TomestoneViewer.GUI;

namespace TomestoneViewer.Character;

public record JobId(uint Id) : IComparable<JobId>
{
    public enum RoleId
    {
       Tank, Melee, Ranged, Caster, RHealer, SHealer, Other, NoJobStone
    }


    public static readonly JobId Empty = new(0);

    public Icon Icon => new(Service.TextureProvider.GetFromGameIcon(new GameIconLookup(this == Empty ? 62143 : this.Id + 62100)).GetWrapOrEmpty().Handle, 25);

    public Icon SmallIcon => new(Service.TextureProvider.GetFromGameIcon(new GameIconLookup(this == Empty ? 62143 : this.Id + 62225)).GetWrapOrEmpty().Handle, 25*(1-0.15f*2), 0.15f, 1 - 0.15f);


    public RoleId GetRoleId()
    {
        switch (this.Id)
        {
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 26:
            case 29:
            case 36:
                return JobId.RoleId.NoJobStone;
            case 19:
            case 21:
            case 32:
            case 37:
                return JobId.RoleId.Tank;
            case 20:
            case 22:
            case 30:
            case 34:
            case 39:
            case 41:
                return JobId.RoleId.Melee;
            case 23:
            case 31:
            case 38:
                return JobId.RoleId.Ranged;
            case 24:
            case 33:
                return JobId.RoleId.RHealer;
            case 25:
            case 27:
            case 35:
            case 42:
                return JobId.RoleId.Caster;
            case 28:
            case 40:
                return JobId.RoleId.SHealer;
            default:
                return JobId.RoleId.Other; 
        }
    }

    public static JobId FromFFLogsString(string? value)
    {
        if (value == null)
        {
            return Empty;
        }

        switch (value)
        {
            case "Gladiator": return new JobId(1);
            case "Pugilist": return new JobId(2);
            case "Marauder": return new JobId(3);
            case "Lancer": return new JobId(4);
            case "Archer": return new JobId(5);
            case "Conjurer": return new JobId(6);
            case "Thaumaturge": return new JobId(7);
            case "Paladin": return new JobId(19);
            case "Monk": return new JobId(20);
            case "Warrior": return new JobId(21);
            case "Dragoon": return new JobId(22);
            case "Bard": return new JobId(23);
            case "WhiteMage": return new JobId(24);
            case "BlackMage": return new JobId(25);
            case "Arcanist": return new JobId(26);
            case "Summoner": return new JobId(27);
            case "Scholar": return new JobId(28);
            case "Rogue": return new JobId(29);
            case "Ninja": return new JobId(30);
            case "Machinist": return new JobId(31);
            case "DarkKnight": return new JobId(32);
            case "Astrologian": return new JobId(33);
            case "Samurai": return new JobId(34);
            case "RedMage": return new JobId(35);
            case "BlueMage": return new JobId(36);
            case "Gunbreaker": return new JobId(37);
            case "Dancer": return new JobId(38);
            case "Reaper": return new JobId(39);
            case "Sage": return new JobId(40);
            case "Viper": return new JobId(41);
            case "Pictomancer": return new JobId(42);
            default: return Empty;
        }
    }

    public int CompareTo(JobId? other)
    {
        if (other == null)
        {
            return 0;
        }
        else
        {
            return this.Id.CompareTo(other.Id);
        }
    }

    public override string ToString()
    {
        return this.Id.ToString();
    }
}
