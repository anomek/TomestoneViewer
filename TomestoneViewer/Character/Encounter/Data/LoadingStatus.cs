using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomestoneViewer.Character.Encounter.Data;

public class LoadingStatus
{
    public bool Loading { get; private set; }

    public IEncounterDataError? Error { get; private set; }

    public LoadingStatus()
    {
        this.LoadingStarted();
    }

    public void LoadingStarted()
    {
        this.Loading = true;
        this.Error = null;
    }

    public void LoadingDone()
    {
        this.Loading = false;
        this.Error = null;
    }

    public void LoadingError(IEncounterDataError error)
    {
        this.Loading = false;
        this.Error = error;
    }
}
