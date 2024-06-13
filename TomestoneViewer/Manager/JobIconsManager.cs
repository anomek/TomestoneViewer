using System;
using System.Collections.Generic;
using System.Numerics;

using TomestoneViewer.Character;
using TomestoneViewer.GUI;

using static Dalamud.Plugin.Services.ITextureProvider;

namespace TomestoneViewer.Manager;

public class JobIconsManager : IDisposable
{
    private static readonly IconClass JobIcon = new(new Vector2(64), new Vector2(0), IconFlags.HiRes, 62143, 62101);
    private static readonly IconClass SmallJobIcon = new(new Vector2(22), new Vector2(5), IconFlags.None, 62143, 62226);
    private static readonly IReadOnlyList<IconClass> IconClasses = [JobIcon, SmallJobIcon];

    private static readonly int JobsCount = 40;

    private readonly object locker = new();

    private Dictionary<IconClass, IReadOnlyList<Icon>>? jobIcons;

    private int iconLoadAttemptsLeft = 4;

    public Icon? GetJobIcon(JobId jobId)
    {
        return this.Get(JobIcon, jobId);
    }

    public Icon? GetJobIconSmall(JobId jobId)
    {
        return this.Get(SmallJobIcon, jobId);
    }

    public void Dispose()
    {
        if (this.jobIcons == null)
        {
            return;
        }

        foreach (var iconClass in this.jobIcons)
        {
            foreach (var icon in iconClass.Value)
            {
                icon.Dispose();
            }
        }
    }

    private Icon? Get(IconClass iconClass, JobId jobId)
    {
        this.EnsureIconsLoaded();
        if (this.jobIcons == null)
        {
            return null;
        }

        var list = this.jobIcons[iconClass];
        var jobIndex = (int)jobId.Id;
        if (list == null || list.Count != JobsCount + 1 || jobIndex > JobsCount)
        {
            return null;
        }

        return list[jobIndex];
    }

    private bool EnsureIconsLoaded()
    {
        if (this.jobIcons == null)
        {
            lock (this.locker)
            {
                if (this.jobIcons == null && this.iconLoadAttemptsLeft > 0)
                {
                    this.iconLoadAttemptsLeft++;
                    this.jobIcons = LoadJobIcons();
                }
            }
        }

        return this.jobIcons != null;
    }

    private static Dictionary<IconClass, IReadOnlyList<Icon>>? LoadJobIcons()
    {
        Dictionary<IconClass, IReadOnlyList<Icon>> jobIcons = [];
        foreach (var iconClass in IconClasses)
        {
            var loaded = iconClass.LoadAll();
            if (loaded == null)
            {
                return null;
            }

            jobIcons[iconClass] = loaded;
        }

        return jobIcons;
    }

    private class IconClass(Vector2 size, Vector2 topLeftCorner, IconFlags iconFlags, uint defaultIcon, uint firstIcon)
    {
        private readonly Vector2 size = size;
        private readonly Vector2 topLeftCorner = topLeftCorner;
        private readonly IconFlags iconFlags = iconFlags;
        private readonly uint defaultIcon = defaultIcon;
        private readonly uint firstIcon = firstIcon;

        internal IReadOnlyList<Icon>? LoadAll()
        {
            List<Icon> icons = [];
            var defaultIcon = this.Load(this.defaultIcon);
            if (defaultIcon == null)
            {
                return null;
            }

            icons.Add(defaultIcon);

            for (uint i = 0; i < JobsCount; i++)
            {
                var icon = this.Load(this.firstIcon + i);
                if (icon == null)
                {
                    return null;
                }

                icons.Add(icon);
            }

            return icons;
        }

        private Icon? Load(uint iconId)
        {
            var icon = Service.TextureProvider.GetIcon(iconId, this.iconFlags);
            if (icon != null)
            {
                return new Icon(icon, this.size, this.topLeftCorner);
            }
            else
            {
                return null;
            }
        }
    }
}
