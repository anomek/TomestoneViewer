using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ImGuiNET;
using TomestoneViewer.Character.Encounter;
using TomestoneViewer.Model;

namespace TomestoneViewer.Character;

public class CharSelector
{
    private readonly CharacterId? characterId;
    private readonly CharacterSelectorError? error;

    internal CharacterId? CharId { get => this.characterId; }

    internal CharacterSelectorError? Error { get => this.error; }

    private CharSelector(CharacterId charId)
    {
        this.characterId = charId;
    }

    private CharSelector(string firstName, string lastName, string world)
    {
        this.characterId = new CharacterId(firstName, lastName, world);
    }

    private CharSelector(CharacterSelectorError charError)
    {
        this.error = charError;
    }

    public override string ToString()
    {
        return this.characterId?.ToString() ?? this.error.Message;
    }

    public static CharSelector SelectCurrentTarget()
    {
        var target = Service.TargetManager.Target;
        if (target is PlayerCharacter targetCharacter && target.ObjectKind != ObjectKind.Companion)
        {
            return FromPlayerCharacter(targetCharacter);
        }
        else
        {
            return new(CharacterSelectorError.InvalidTarget);
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
                return new(CharacterSelectorError.ClipboardError);
            }
        }
        catch
        {
            return new(CharacterSelectorError.ClipboardError);
        }

        return SelectByName(clipboardRawText);
    }

    public static CharSelector SelectFromSearch(SearchCharacterId id)
    {
        return new CharSelector(id.ToCharacterId());
    }

    public static CharSelector SelectByName(string rawText)
    {
        return new CharSelector(CharacterSelectorError.Unimplemented);
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
            return new CharSelector(CharacterSelectorError.EmptyHomeWorld);
        }

        var firstName = playerCharacter.Name.TextValue.Split(' ')[0];
        var lastName = playerCharacter.Name.TextValue.Split(' ')[1];
        var worldName = playerCharacter.HomeWorld.GameData.Name.ToString();
        return new CharSelector(firstName, lastName, worldName);
    }
}