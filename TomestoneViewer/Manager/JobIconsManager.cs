using System.Collections.Generic;
using System.Threading.Tasks;

using Dalamud.Interface.Internal;
using TomestoneViewer.Character;

namespace TomestoneViewer.Manager;

public class JobIconsManager
{
    private List<IDalamudTextureWrap>? jobIcons;
    private volatile bool isLoading;
    private int iconLoadAttemptsLeft = 4;

    public IDalamudTextureWrap? GetJobIcon(JobId jobId)
    {
        if (this.isLoading)
        {
            return null;
        }

        if (this.jobIcons == null)
        {
            this.LoadJobIcons();
        }

        var jobIndex = (int)jobId.Id;
        if (this.jobIcons is { Count: 41 } && jobIndex <= 40)
        {
            return this.jobIcons[jobIndex];
        }

        return null;
    }

    private void LoadJobIcons()
    {
        if (this.iconLoadAttemptsLeft <= 0)
        {
            return;
        }

        this.jobIcons = [];
        this.isLoading = true;
        var hasFailed = false;

        Task.Run(() =>
        {
            var defaultIcon = Service.TextureProvider.GetIcon(62143);
            if (defaultIcon != null)
            {
                this.jobIcons.Add(defaultIcon);
            }
            else
            {
                hasFailed = true;
            }

            for (uint i = 62101; i <= 62140 && !hasFailed; i++)
            {
                var icon = Service.TextureProvider.GetIcon(i);
                if (icon != null)
                {
                    this.jobIcons.Add(icon);
                }
                else
                {
                    hasFailed = true;
                }
            }

            if (hasFailed)
            {
                this.jobIcons = null;

                Service.PluginLog.Error($"Job icons loading failed, {--this.iconLoadAttemptsLeft} attempt(s) left.");
            }

            this.isLoading = false;
        });
    }
}
