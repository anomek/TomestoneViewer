using System.Runtime.InteropServices;

using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Component.GUI;

#pragma warning disable SA1134 // Attributes should not share line
namespace TomestoneViewer.GameSystems;

[Addon("LookingForGroupCondition")]
[StructLayout(LayoutKind.Explicit, Size = 0x364)]
public unsafe partial struct AddonLookingForGroupCondition
{
    [FieldOffset(0)] public AtkUnitBase AtkUnitBase;

    [FieldOffset(0x238)] public AtkComponentDropDownList* DutyCategoryDropDown;
    [FieldOffset(0x240)] public AtkComponentDropDownList* DutyNameDropDown;
    [FieldOffset(0x248)] public AtkComponentTextInput* CommentInput;

    [FieldOffset(0x250)] public AtkTextNode* PartyPasswordInput;

    [FieldOffset(0x258)] public AtkTextNode* UnselectClassesCheckBox;
    [FieldOffset(0x260)] public AtkTextNode* FormPrivatePartyCheckBox;
    [FieldOffset(0x268)] public AtkComponentCheckBox* LimitRecruitingToWorldServerCheckBox;
    [FieldOffset(0x270)] public AtkComponentCheckBox* OnePlayerPerJobCheckBox;

    [FieldOffset(0x278)] public AtkComponentCheckBox* AvgItemLevelCheckBox;
    [FieldOffset(0x280)] public AtkComponentNumericInput* AvgItemLevelInput;

    [FieldOffset(0x288)] public AtkComponentButton* RecruitMembersButton;
    [FieldOffset(0x290)] public AtkComponentButton* CancelButton;

    [FieldOffset(0x298)] public AtkComponentDropDownList* DropDown3;
    [FieldOffset(0x2a0)] public AtkComponentButton* ResetButton;

    [FieldOffset(0x2a8)] public AtkComponentCheckBox* RemoveSpectatorsCheckBox;

    [FieldOffset(0x2b0)] public AtkComponentButton* NormalButton;
    [FieldOffset(0x2b8)] public AtkComponentButton* AllianceButton;
    [FieldOffset(0x2c0)] public AtkComponentButton* CustomMatchButton;

    [FieldOffset(0x2c8)] public AtkComponentDropDownList* DropDown4;
    [FieldOffset(0x2d0)] public AtkComponentDropDownList* DropDown5;
    [FieldOffset(0x2d8)] public AtkComponentDropDownList* DropDown6;
    [FieldOffset(0x2e0)] public AtkComponentDropDownList* DropDown7;

    [FieldOffset(0x330)] public AtkComponentCheckBox* CompletionStatusCheckBox;
    [FieldOffset(0x358)] public AtkComponentCheckBox* UnrestrictedPartyCheckBox;
    [FieldOffset(0x360)] public AtkComponentCheckBox* MinimumILCheckBox;
    [FieldOffset(0x368)] public AtkComponentCheckBox* SilenceEchoCheckBox;
}
