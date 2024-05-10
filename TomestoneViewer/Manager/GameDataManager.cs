using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

using TomestoneViewer.Model;

namespace TomestoneViewer.Manager;

public class GameDataManager
{
    public JobIconsManager JobIconsManager { get; } = new();
}
