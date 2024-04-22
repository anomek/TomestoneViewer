using System;
using System.Linq;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Gui.PartyFinder.Types;
using Dalamud.Hooking;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;

namespace TomestoneViewer.Manager;

public unsafe class OpenWithManager
{
    public bool HasLoadingFailed;
    public bool HasBeenEnabled;

    private DateTime wasOpenedLast = DateTime.Now;
    private short isJoiningPartyFinderOffset;
    private nint charaCardAtkCreationAddress;
    private nint processInspectPacketAddress;
    private nint socialDetailAtkCreationAddress;
    private nint processPartyFinderDetailPacketAddress;
    private nint atkUnitBaseFinalizeAddress;

    private delegate void* CharaCardAtkCreationDelegate(nint agentCharaCard);
    private Hook<CharaCardAtkCreationDelegate>? charaCardAtkCreationHook;

    private delegate void* ProcessInspectPacketDelegate(void* someAgent, void* a2, nint packetData);
    private Hook<ProcessInspectPacketDelegate>? processInspectPacketHook;

    private delegate void* SocialDetailAtkCreationDelegate(void* someAgent, nint data, long a3, void* a4);
    private Hook<SocialDetailAtkCreationDelegate>? socialDetailAtkCreationHook;

    private delegate void* ProcessPartyFinderDetailPacketDelegate(nint someAgent, nint packetData);
    private Hook<ProcessPartyFinderDetailPacketDelegate>? processPartyFinderDetailPacketHook;

    private delegate void AtkUnitBaseFinalizeDelegate(AtkUnitBase* addon);
    private Hook<AtkUnitBaseFinalizeDelegate>? atkUnitBaseFinalizeHook;

    public OpenWithManager()
    {
        this.ScanSigs();
        this.EnableHooks();
    }

    public void ReloadState()
    {
        this.EnableHooks();
    }

    public void Dispose()
    {
        this.charaCardAtkCreationHook?.Dispose();
        this.processInspectPacketHook?.Dispose();
        this.socialDetailAtkCreationHook?.Dispose();
        this.processPartyFinderDetailPacketHook?.Dispose();
        this.atkUnitBaseFinalizeHook?.Dispose();
    }

    private static bool IsEnabled()
    {
        return true;

    }

    private void ScanSigs()
    {
        try
        {
            this.charaCardAtkCreationAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 4C 8B 74 24 ?? 48 8B 74 24 ?? 48 8B 5C 24 ?? 48 83 C4 30");
            this.processInspectPacketAddress = Service.SigScanner.ScanText("48 89 5C 24 ?? 56 41 56 41 57 48 83 EC 20 8B DA");
            this.socialDetailAtkCreationAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 48 8B 8B ?? ?? ?? ?? BE");
            this.processPartyFinderDetailPacketAddress = Service.SigScanner.ScanText("E9 ?? ?? ?? ?? CC CC CC CC CC CC 48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 55 48 8D AC 24");
            this.atkUnitBaseFinalizeAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 45 33 C9 8D 57 01 41 B8");

            try
            {
                this.isJoiningPartyFinderOffset = *(short*)(Service.SigScanner.ScanModule("48 8B F1 66 C7 81") + 6);
            }
            catch (Exception ex)
            {
                Service.PluginLog.Error(ex, "isJoiningPartyFinderOffset sig scan failed.");
            }
        }
        catch (Exception ex)
        {
            this.HasLoadingFailed = true;
            Service.PluginLog.Error(ex, "OpenWith sig scan failed.");
        }
    }

    private void EnableHooks()
    {
        if (this.HasLoadingFailed || this.HasBeenEnabled)
        {
            return;
        }

        try
        {
        }
        catch (Exception ex)
        {
            this.HasLoadingFailed = true;
            Service.PluginLog.Error(ex, "OpenWith hooks enabling failed.");
        }

        this.HasBeenEnabled = true;
    }

    private void AktUnitBaseFinalizeDetour(AtkUnitBase* addon)
    {
        try
        {
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, "Exception in AktUnitBaseFinalizeDetour.");
        }

        this.atkUnitBaseFinalizeHook!.Original(addon);
    }
}
