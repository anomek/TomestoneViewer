using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using TomestoneViewer.Model;
using TomestoneViewer.Model.GameData;

namespace TomestoneViewer.Manager;

public class GameDataManager
{
    public static readonly List<Job> Jobs =
    [
        new Job { Name = "All jobs", Id = uint.MaxValue, Color = new Vector4(255, 255, 255, 255) / 255 },
        new Job { Name = "Current job", Id = uint.MaxValue, Color = new Vector4(255, 255, 255, 255) / 255 },
        new Job { Name = "Astrologian", Abbreviation = "AST", Id = 33, Color = new Vector4(255, 231, 74, 255) / 255 },
        new Job { Name = "Bard", Abbreviation = "BRD", Id = 23, Color = new Vector4(145, 150, 186, 255) / 255 },
        new Job { Name = "Black Mage", Abbreviation = "BLM", Id = 25, Color = new Vector4(165, 121, 214, 255) / 255 },
        new Job { Name = "Dancer", Abbreviation = "DNC", Id = 38, Color = new Vector4(226, 176, 175, 255) / 255 },
        new Job { Name = "Dark Knight", Abbreviation = "DRK", Id = 32, Color = new Vector4(209, 38, 204, 255) / 255 },
        new Job { Name = "Dragoon", Abbreviation = "DRG", Id = 22, Color = new Vector4(65, 100, 205, 255) / 255 },
        new Job { Name = "Gunbreaker", Abbreviation = "GNB", Id = 37, Color = new Vector4(121, 109, 48, 255) / 255 },
        new Job { Name = "Machinist", Abbreviation = "MCH", Id = 31, Color = new Vector4(110, 225, 214, 255) / 255 },
        new Job { Name = "Monk", Abbreviation = "MNK", Id = 20, Color = new Vector4(214, 156, 0, 255) / 255 },
        new Job { Name = "Ninja", Abbreviation = "NIN", Id = 30, Color = new Vector4(175, 25, 100, 255) / 255 },
        new Job { Name = "Paladin", Abbreviation = "PLD", Id = 19, Color = new Vector4(168, 210, 230, 255) / 255 },
        new Job { Name = "Red Mage", Abbreviation = "RDM", Id = 35, Color = new Vector4(232, 123, 123, 255) / 255 },
        new Job { Name = "Reaper", Abbreviation = "RPR", Id = 39, Color = new Vector4(150, 90, 144, 255) / 255 },
        new Job { Name = "Sage", Abbreviation = "SGE", Id = 40, Color = new Vector4(128, 160, 240, 255) / 255 },
        new Job { Name = "Samurai", Abbreviation = "SAM", Id = 34, Color = new Vector4(228, 109, 4, 255) / 255 },
        new Job { Name = "Scholar", Abbreviation = "SCH", Id = 28, Color = new Vector4(134, 87, 255, 255) / 255 },
        new Job { Name = "Summoner", Abbreviation = "SMN", Id = 27, Color = new Vector4(45, 155, 120, 255) / 255 },
        new Job { Name = "Warrior", Abbreviation = "WAR", Id = 21, Color = new Vector4(207, 38, 33, 255) / 255 },
        new Job { Name = "White Mage", Abbreviation = "WHM", Id = 24, Color = new Vector4(255, 240, 220, 255) / 255 },
    ];

    public volatile bool IsDataReady;
    public volatile bool IsDataLoading;
    public volatile bool HasFailed;
    public GameData? GameData;
    public JobIconsManager JobIconsManager = new();

    public void FetchData()
    {
        if (this.IsDataLoading)
        {
            return;
        }

        this.IsDataReady = false;
        this.IsDataLoading = true;
        Task.Run(async () => {
        }).ContinueWith(t =>
        {
            if (!this.IsDataReady)
            {
                this.HasFailed = true;
            }

            this.IsDataLoading = false;
            if (!t.IsFaulted) return;
            if (t.Exception == null) return;
            foreach (var e in t.Exception.Flatten().InnerExceptions)
            {
                Service.PluginLog.Error(e, "Network error.");
            }
        });
    }

    public void SetDataFromJson(string jsonContent)
    {
        var gameData = GameData.FromJson(jsonContent);
        if (gameData == null)
        {
            Service.PluginLog.Error("gameData was null while fetching game data");
        }
        else if (gameData.Errors == null)
        {
            if (gameData.IsDataValid())
            {
                this.GameData = gameData;
                this.IsDataReady = true;
            }
        }
        else
        {
            Service.PluginLog.Error("Errors while fetching game data: " + gameData.Errors.Message);
        }
    }
    private static List<Job> GetJobs()
    {
        return
        [
            new Job { Name = "All jobs", Id = uint.MaxValue, Color = new Vector4(255, 255, 255, 255) / 255 },
            new Job { Name = "Current job", Id = uint.MaxValue, Color = new Vector4(255, 255, 255, 255) / 255 },
            new Job { Name = "Astrologian", Abbreviation = "AST", Id = 33, Color = new Vector4(255, 231, 74, 255) / 255 },
            new Job { Name = "Bard", Abbreviation = "BRD", Id = 23, Color = new Vector4(145, 150, 186, 255) / 255 },
            new Job { Name = "Black Mage", Abbreviation = "BLM", Id = 25, Color = new Vector4(165, 121, 214, 255) / 255 },
            new Job { Name = "Dancer", Abbreviation = "DNC", Id = 38, Color = new Vector4(226, 176, 175, 255) / 255 },
            new Job { Name = "Dark Knight", Abbreviation = "DRK", Id = 32, Color = new Vector4(209, 38, 204, 255) / 255 },
            new Job { Name = "Dragoon", Abbreviation = "DRG", Id = 22, Color = new Vector4(65, 100, 205, 255) / 255 },
            new Job { Name = "Gunbreaker", Abbreviation = "GNB", Id = 37, Color = new Vector4(121, 109, 48, 255) / 255 },
            new Job { Name = "Machinist", Abbreviation = "MCH", Id = 31, Color = new Vector4(110, 225, 214, 255) / 255 },
            new Job { Name = "Monk", Abbreviation = "MNK", Id = 20, Color = new Vector4(214, 156, 0, 255) / 255 },
            new Job { Name = "Ninja", Abbreviation = "NIN", Id = 30, Color = new Vector4(175, 25, 100, 255) / 255 },
            new Job { Name = "Paladin", Abbreviation = "PLD", Id = 19, Color = new Vector4(168, 210, 230, 255) / 255 },
            new Job { Name = "Red Mage", Abbreviation = "RDM", Id = 35, Color = new Vector4(232, 123, 123, 255) / 255 },
            new Job { Name = "Reaper", Abbreviation = "RPR", Id = 39, Color = new Vector4(150, 90, 144, 255) / 255 },
            new Job { Name = "Sage", Abbreviation = "SGE", Id = 40, Color = new Vector4(128, 160, 240, 255) / 255 },
            new Job { Name = "Samurai", Abbreviation = "SAM", Id = 34, Color = new Vector4(228, 109, 4, 255) / 255 },
            new Job { Name = "Scholar", Abbreviation = "SCH", Id = 28, Color = new Vector4(134, 87, 255, 255) / 255 },
            new Job { Name = "Summoner", Abbreviation = "SMN", Id = 27, Color = new Vector4(45, 155, 120, 255) / 255 },
            new Job { Name = "Warrior", Abbreviation = "WAR", Id = 21, Color = new Vector4(207, 38, 33, 255) / 255 },
            new Job { Name = "White Mage", Abbreviation = "WHM", Id = 24, Color = new Vector4(255, 240, 220, 255) / 255 },
        ];
    }
}
