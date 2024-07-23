using System;
using System.Collections.Generic;

using Dalamud.Game.Addon.Events;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
namespace TomestoneViewer.GameSystems;

public unsafe class AddonInterface<TAddon> : IDisposable
{
    private readonly IAddonLifecycle.AddonEventDelegate onAddonStartListener;
    private readonly IAddonLifecycle.AddonEventDelegate onAddonEndListener;
    private readonly List<IAddonEventHandle> internalRegisteredEventHandlers = [];
    private readonly List<EventRegistration<TAddon>> eventRegistrations = [];

    private readonly List<Callback<TAddon>> onStartListeners = [];
    private readonly List<Callback<TAddon>> onEndListeners = [];

    public delegate void Callback<T>(T* addon);

    public delegate nint ComponentSelector<T>(T* addon);

    private record EventRegistration<T>(ComponentSelector<T> Component, AddonEventType EventType, Callback<T> Callback)
    {
        internal nint GetComponent(T* addon)
        {
            return this.Component.Invoke(addon);
        }
    }

    public AddonInterface(string name)
    {
        this.onAddonStartListener = new IAddonLifecycle.AddonEventDelegate(this.OnAddonStart);
        this.onAddonEndListener = new IAddonLifecycle.AddonEventDelegate(this.OnAddonEnd);
        Service.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, name, this.OnAddonStart);
        Service.AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, name, this.OnAddonEnd);
    }

    public void Dispose()
    {
        Service.AddonLifecycle.UnregisterListener(this.onAddonStartListener, this.onAddonEndListener);
        this.internalRegisteredEventHandlers.ForEach(Service.AddonEventManager.RemoveEvent);
    }

    public void AddOnStartHandler(Callback<TAddon> onStart)
    {
        this.onStartListeners.Add(onStart);
    }

    public void AddOnEndHandler(Callback<TAddon> onStart)
    {
        this.onEndListeners.Add(onStart);
    }

    public void AddEventHandler(ComponentSelector<TAddon> component, AddonEventType eventType, Callback<TAddon> callback)
    {
        this.eventRegistrations.Add(new(component, eventType, callback));
    }

    private void OnAddonStart(AddonEvent type, AddonArgs args)
    {
        var addon = (TAddon*)args.Addon;
        this.internalRegisteredEventHandlers.ForEach(Service.AddonEventManager.RemoveEvent);
        this.internalRegisteredEventHandlers.Clear();
        this.eventRegistrations.ForEach(registration =>
        {
            var handle = Service.AddonEventManager.AddEvent(args.Addon, registration.GetComponent(addon), registration.EventType, (_, addonPtr, _) => registration.Callback.Invoke((TAddon*)addonPtr));
            if (handle != null)
            {
                this.internalRegisteredEventHandlers.Add(handle);
            }
            else
            {
                Service.PluginLog.Error("Failed to add event handler");
            }
        });

        this.onStartListeners.ForEach(listener => listener.Invoke(addon));
    }

    private void OnAddonEnd(AddonEvent type, AddonArgs args)
    {
        this.internalRegisteredEventHandlers.ForEach(Service.AddonEventManager.RemoveEvent);
        this.internalRegisteredEventHandlers.Clear();

        this.onEndListeners.ForEach(listener => listener.Invoke((TAddon*)args.Addon));
    }
}
