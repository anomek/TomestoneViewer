using System;
using System.IO;

using Newtonsoft.Json;

namespace TomestoneViewer.External;

internal class FFLogsViewerConfigReader(string path)
{
    private readonly string path = path;

    internal bool HasFFlogs { get => this.ClientId != null && this.ClientSecret != null; }

    internal string? ClientId { get; private set; }

    internal string? ClientSecret { get; private set; }

    internal void Refresh()
    {
        this.ClientId = null;
        this.ClientSecret = null;
        var top = Path.GetDirectoryName(this.path);
        if (top != null)
        {
            var full = top + "\\FFLogsViewer.json";

            try
            {
                var json = File.ReadAllText(full);
                var obj = (dynamic?)JsonConvert.DeserializeObject(json);
                this.ClientId = obj?.ClientId;
                this.ClientSecret = obj?.ClientSecret;
            }
            catch (FileNotFoundException)
            {
                // ignore
            }
            catch (Exception ex)
            {
                Service.PluginLog.Error($"Error while reading file {full}", ex);
            }
        }
    }
}
