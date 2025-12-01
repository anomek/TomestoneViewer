using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomestoneViewer.External;

public class PlayerTrackInterface
{
    private readonly ICallGateSubscriber<string, uint, string> subscriber;

    internal bool Available { get => this.subscriber.HasFunction; }

    internal PlayerTrackInterface(IDalamudPluginInterface pluginInterface)
    {
        this.subscriber = pluginInterface.GetIpcSubscriber<string, uint, string>("PlayerTrack.GetPlayerNotes");
    }

    internal string GetPlayerNotes(string name, uint world)
    {
        return this.subscriber.InvokeFunc(name, world);
    }
}
