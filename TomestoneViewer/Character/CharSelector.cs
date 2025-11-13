using System;
using System.Linq;
using System.Text.RegularExpressions;

using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Dalamud.Bindings.ImGui;
using TomestoneViewer.Model;

namespace TomestoneViewer.Character;

public class CharSelector
{
    internal CharacterId? CharId { get; }

    internal CharacterSelectorError? Error { get; }

    private CharSelector(CharacterId charId)
    {
        this.CharId = charId;
    }

    private CharSelector(string firstName, string lastName, string world)
    {
        this.CharId = new CharacterId(firstName, lastName, world);
    }

    private CharSelector(CharacterSelectorError charError)
    {
        this.Error = charError;
    }

    internal bool IsValid()
    {
        return this.CharId != null;
    }

    public override string ToString()
    {
        return this.CharId?.ToString() ?? this.Error?.Message ?? "CharSelector inconsistent state";
    }

    public static CharSelector SelectCurrentTarget()
    {
        var target = Service.TargetManager.Target;
        if (target is IPlayerCharacter targetCharacter && target.ObjectKind != ObjectKind.Companion)
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
        var asPlaceholder = ResolvePlaceholder(rawText);
        if (asPlaceholder != null)
        {
            return new CharSelector(asPlaceholder);
        }

        var asCharacterId = CharacterId.FromQualifiedName(SanitizeRawText(rawText));
        if (asCharacterId != null)
        {
            return new CharSelector(asCharacterId);
        }

        return new CharSelector(CharacterSelectorError.InvalidCharacterName);
    }

    public static CharSelector SelectById(CharacterId? characterId)
    {
        if (characterId == null)
        {
            return new CharSelector(CharacterSelectorError.InvalidCharacterName);
        }

        return new CharSelector(characterId);
    }

    private static string SanitizeRawText(string rawText)
    {
        rawText = rawText.Replace("'s party for", " ");
        rawText = rawText.Replace("You join", " ");
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
        rawText = Regex.Replace(rawText, @"\[.*?\]", " ");
        rawText = Regex.Replace(rawText, "[^A-Za-z '-]", " ");
        rawText = string.Concat(rawText.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
        rawText = Regex.Replace(rawText, @"\s+", " ");
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
        return rawText;
    }

    private static unsafe CharacterId? ResolvePlaceholder(string text)
    {
        try
        {
            var placeholder = Framework.Instance()->GetUIModule()->GetPronounModule()->ResolvePlaceholder(text, 0, 0);
            if (placeholder != null && placeholder->IsCharacter())
            {
                var character = (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)placeholder;
                var world = Service.GameData.GetWorldName(character->HomeWorld);

                if (world != null && placeholder->Name != null)
                {
                    return CharacterId.FromFullName(Util.ReadSeString(placeholder->GetName()).TextValue, world);
                }
            }
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, "Error while resolving placeholder.");
            return null;
        }

        return null;
    }

    private static CharSelector FromPlayerCharacter(IPlayerCharacter playerCharacter)
    {
        if (!playerCharacter.HomeWorld.ValueNullable.HasValue)
        {
            Service.PluginLog.Error("SetInfo character world was null");
            return new CharSelector(CharacterSelectorError.EmptyHomeWorld);
        }

        var charId = CharacterId.FromFullName(playerCharacter.Name.TextValue, playerCharacter.HomeWorld.Value.Name.ToString());
        if (charId != null)
        {
            return new CharSelector(charId);
        }
        else
        {
            return new CharSelector(CharacterSelectorError.InvalidCharacterName);
        }
    }

}
