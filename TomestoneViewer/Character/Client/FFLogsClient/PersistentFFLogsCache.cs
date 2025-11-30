using FFXIVClientStructs.FFXIV.Component.Text;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomestoneViewer.Character.Client.TomestoneClient;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.Client.FFLogsClient;
internal class PersistentFFLogsCache
{
    private static readonly int VERSION = 2;
    private readonly string path;

    private IDictionary<(LodestoneId UserId, int BossId), FFLogsEncounterData>? cache;

    internal PersistentFFLogsCache(string path)
    {
        this.path = path;
        this.Load();
    }

    internal async Task<ClientResponse<FFLogsClientError, FFLogsEncounterData>> Get(LodestoneId lodestoneId, int bossId, Func<Task<ClientResponse<FFLogsClientError, FFLogsEncounterData>>> query)
    {
        var key = (lodestoneId, bossId);
        if (this.cache != null && this.cache.TryGetValue(key, out var data))
        {
            return new(data);
        }
        else
        {
            var response = await query.Invoke();
            if (this.cache != null)
            {
                response.OnSuccess(success =>
                {
                    this.cache[key] = success;
                    this.Save();
                });
            }

            return response;
        }
    }


    private void Load()
    {
        var copy = new Dictionary<(LodestoneId UserId, int BossId), FFLogsEncounterData>();
        Stream? stream = null;
        BinaryReader? reader = null;
        try
        {
            if (!File.Exists(this.path))
            {
                Service.PluginLog.Info($"FFLogs cache file {this.path} does not exist");
                return;
            }

            stream = File.Open(this.path, FileMode.Open, FileAccess.Read, FileShare.Read);
            reader = new BinaryReader(stream);
            var version = reader.ReadInt32();
            if (version != VERSION)
            {
                Service.PluginLog.Info($"FFLogs cache file version {version} doesn't match curerent version {VERSION}");
                return;
            }

            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                var userId = new LodestoneId(reader.ReadUInt32());
                var bossId = reader.ReadInt32();

                var count2 = reader.ReadInt32();
                var encounterData = new Dictionary<JobId, FFLogsEncounterData.CClearCount>();
                for (var i2 = 0; i2 < count2; i2++)
                {
                    var jobId = new JobId(reader.ReadUInt32());
                    var clears = reader.ReadUInt32();
                    var lastClear = DateTimeOffset.FromUnixTimeMilliseconds(reader.ReadInt64());

                    encounterData[jobId] = new FFLogsEncounterData.CClearCount(0, clears, lastClear);
                }

                copy[(userId, bossId)] = new FFLogsEncounterData(encounterData);
            }
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, "Failed to read cache file");
        }
        finally
        {
            stream?.Close();
            reader?.Close();
            this.cache = new ConcurrentDictionary<(LodestoneId UserId, int BossId), FFLogsEncounterData>(copy);
        }
    }


    private void Save()
    {
        var copy = new Dictionary<(LodestoneId UserId, int BossId), FFLogsEncounterData>(this.cache);
        var tempPath = this.path + ".tmp";
        Stream? stream = null;
        BinaryWriter? writer = null;
        try
        {
            stream = File.Open(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.Write);
            writer = new(stream);
            writer.Write(VERSION);
            writer.Write(copy.Count);
            foreach (var item in copy)
            {
                writer.Write(item.Key.UserId.Id);
                writer.Write(item.Key.BossId);
                writer.Write(item.Value.ClearsPerJob.Count);
                foreach (var clear in item.Value.ClearsPerJob)
                {
                    writer.Write(clear.Key.Id);
                    writer.Write(clear.Value.PreviousExpansions);
                    writer.Write(clear.Value.LastClear.ToUnixTimeMilliseconds());
                }
            }
        }
        finally
        {
            stream?.Close();
            writer?.Close();
        }

        File.Move(tempPath, this.path, true);
    }
}
