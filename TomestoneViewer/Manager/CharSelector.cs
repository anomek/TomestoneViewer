using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using TomestoneViewer.Model;
using ImGuiNET;

namespace TomestoneViewer.Manager;

public class CharSelector
{
    public CharacterId? CharId { get; private set; }

    public CharacterError? CharError { get; private set; }

    internal CharSelector(CharacterId charId)
    {
        this.CharId = charId;
    }

    internal CharSelector(string firstName, string lastName, string world)
    {
        this.CharId = new CharacterId(firstName, lastName, world);
    }

    internal CharSelector(CharacterError charError)
    {
        this.CharError = charError;
    }

    public static CharSelector SelectCurrentTarget()
    {
        var target = Service.TargetManager.Target;
        if (target is PlayerCharacter targetCharacter && target.ObjectKind != ObjectKind.Companion)
        {
            return CharSelector.FromPlayerCharacter(targetCharacter);
        }
        else
        {
            return new CharSelector(CharacterError.InvalidTarget);
        }
    }

    public static CharSelector SelectFromClipboard()
    {
        string clipboardRawText;
        try
        {
            clipboardRawText = ImGui.GetClipboardText();
            if (clipboardRawText == null)
            {
                return new CharSelector(CharacterError.ClipboardError);
            }
        }
        catch
        {
            return new CharSelector(CharacterError.ClipboardError);
        }

        return CharSelector.SelectByName(clipboardRawText);
    }

    public static CharSelector SelectFromSearch(SearchCharacterId id)
    {
        return new CharSelector(id.ToCharacterId());
    }

    public static CharSelector SelectByName(string rawText)
    {
        return new CharSelector(CharacterError.Unimplemented);
    }

    public static CharSelector SelectByName(string fullName, string world)
    {
        var firstName = fullName.Split(' ')[0];
        var lastName = fullName.Split(' ')[1];
        return new CharSelector(firstName, lastName, world);
    }

    public static CharSelector SelectByName(string firstName, string lastName, string world)
    {
        return new CharSelector(firstName, lastName, world);
    }

    private static CharSelector FromPlayerCharacter(PlayerCharacter playerCharacter)
    {
        if (playerCharacter.HomeWorld.GameData?.Name == null)
        {
            Service.PluginLog.Error("SetInfo character world was null");
            return new CharSelector(CharacterError.GenericError);
        }

        var firstName = playerCharacter.Name.TextValue.Split(' ')[0];
        var lastName = playerCharacter.Name.TextValue.Split(' ')[1];
        var worldName = playerCharacter.HomeWorld.GameData.Name.ToString();
        return new CharSelector(firstName, lastName, worldName);
    }
}
