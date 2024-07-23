using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using Dalamud.Interface;
using Dalamud.Interface.Utility;
using ImGuiNET;
using TomestoneViewer.Character;
using TomestoneViewer.Manager;

namespace TomestoneViewer.GUI.Main;

public class HeaderBar(IReadOnlyList<CharData> partyList)
{
    private readonly IReadOnlyList<CharData> partyList = partyList;

    public void Draw()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4 * ImGuiHelpers.GlobalScale, ImGui.GetStyle().ItemSpacing.Y));

        var buttonsWidth = GetButtonsWidth();

        var contentRegionAvailWidth = ImGui.GetContentRegionAvail().X;

        var calcInputSize = (contentRegionAvailWidth - (ImGui.GetStyle().ItemSpacing.X * 2) - buttonsWidth) / 3;

        ImGui.SetNextItemWidth(calcInputSize);
        ImGui.InputTextWithHint("##FirstName", "First Name", ref Service.CharDataManager.SearchCharacterId.FirstName, 15, ImGuiInputTextFlags.CharsNoBlank);

        ImGui.SameLine();
        ImGui.SetNextItemWidth(calcInputSize);
        ImGui.InputTextWithHint("##LastName", "Last Name", ref Service.CharDataManager.SearchCharacterId.LastName, 15, ImGuiInputTextFlags.CharsNoBlank);

        ImGui.SameLine();
        ImGui.SetNextItemWidth(calcInputSize);
        ImGui.InputTextWithHint("##WorldName", "World", ref Service.CharDataManager.SearchCharacterId.World, 15, ImGuiInputTextFlags.CharsNoBlank);

        ImGui.SameLine();
        if (Util.DrawButtonIcon(FontAwesomeIcon.Search))
        {
            Service.CharDataManager.SetCharacter(CharSelector.SelectFromSearch(Service.CharDataManager.SearchCharacterId));
        }

        Util.SetHoverTooltip("Search");

        ImGui.SameLine();
        if (Util.DrawButtonIcon(FontAwesomeIcon.Crosshairs))
        {
            Service.CharDataManager.SetCharacter(CharSelector.SelectCurrentTarget());
        }

        Util.SetHoverTooltip("Target");

        ImGui.SameLine();
        if (Util.DrawButtonIcon(FontAwesomeIcon.Clipboard))
        {
            Service.CharDataManager.SetCharacter(CharSelector.SelectFromClipboard());
        }

        Util.SetHoverTooltip("Search clipboard");

        ImGui.SameLine();
        if (Util.DrawButtonIcon(FontAwesomeIcon.UsersCog))
        {
            ImGui.OpenPopup("##TeamList");
        }

        Util.SetHoverTooltip("Party members");

        this.DrawPartyMembersPopup();

        ImGui.PopStyleVar();

        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 2);

        var characterError = Service.CharDataManager.CharacterError;
        var displayChar = Service.CharDataManager.DisplayedChar;

        if (characterError != null)
        {
            if (displayChar != null)
            {
                Util.CenterSelectableError(characterError);
                Util.LinkOpenOrPopup(displayChar);
            }
            else
            {
                Util.CenterError(characterError);
            }
        }
        else if (displayChar != null)
        {
            Util.CenterSelectable($"{displayChar.CharId}");
            Util.LinkOpenOrPopup(displayChar);
        }
    }

    public static float GetMinWindowSize()
    {
        return ((GetMinInputWidth() + (ImGui.GetStyle().ItemSpacing.X * 2)) * 3) + GetButtonsWidth();
    }

    private static float GetButtonsWidth()
    {
        ImGui.PushFont(UiBuilder.IconFont);
        var buttonsWidth =
            ImGui.CalcTextSize(FontAwesomeIcon.Search.ToIconString()).X +
            ImGui.CalcTextSize(FontAwesomeIcon.Crosshairs.ToIconString()).X +
            ImGui.CalcTextSize(FontAwesomeIcon.Clipboard.ToIconString()).X +
            ImGui.CalcTextSize(FontAwesomeIcon.UsersCog.ToIconString()).X +
            (ImGui.GetStyle().ItemSpacing.X * 4) + // between items
            (ImGui.GetStyle().FramePadding.X * 8); // around buttons, 2 per
        ImGui.PopFont();
        return buttonsWidth;
    }

    private static float GetMinInputWidth()
    {
        return new[]
        {
            ImGui.CalcTextSize("First Name").X,
            ImGui.CalcTextSize("Last Name").X,
            ImGui.CalcTextSize("World").X,
        }.Max() + (ImGui.GetStyle().FramePadding.X * 2);
    }

    private void DrawPartyMembersPopup()
    {
        if (!ImGui.BeginPopup("##TeamList", ImGuiWindowFlags.NoMove))
        {
            return;
        }

        if (this.partyList.Count > 0)
        {
            if (ImGui.BeginTable("##PartyListTable", 3, ImGuiTableFlags.RowBg))
            {
                for (var i = 0; i < this.partyList.Count; i++)
                {
                    if (i != 0)
                    {
                        ImGui.TableNextRow();
                    }

                    ImGui.TableNextColumn();

                    var partyMember = this.partyList[i];
                    var iconSize = (float)Math.Round(25 * ImGuiHelpers.GlobalScale); // round because of shaking issues
                    var middleCursorPosY = ImGui.GetCursorPosY() + (iconSize / 2) - (ImGui.GetFontSize() / 2);

                    if (ImGui.Selectable($"##PartyListSel{i}", false, ImGuiSelectableFlags.SpanAllColumns, new Vector2(0, iconSize)))
                    {
                        Service.CharDataManager.SetCharacter(CharSelector.SelectById(partyMember.CharId));
                    }

                    ImGui.SameLine();
                    ImGui.Image(partyMember.JobId.ImGuiIconHandle, new Vector2(iconSize));

                    ImGui.TableNextColumn();

                    ImGui.SetCursorPosY(middleCursorPosY);
                    ImGui.Text(partyMember.CharId.FullName);

                    ImGui.TableNextColumn();

                    ImGui.SetCursorPosY(middleCursorPosY);
                    ImGui.Text(partyMember.CharId.World + " ");
                }

                ImGui.EndTable();
            }
        }
        else
        {
            ImGui.Text("No party member found");
        }

        ImGui.EndPopup();
    }
}
