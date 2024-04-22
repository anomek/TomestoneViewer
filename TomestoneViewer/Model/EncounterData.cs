using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomestoneViewer.Model;

public class EncounterData
{
    public CStatus Status { get; private set; } = new();

    public CData Data { get; private set; } = new();

    public class CStatus
    {
        public bool Loading { get; private set; } = false;

        public bool Loaded { get; private set; } = false;

        public CharacterError? Error { get; private set; } = null;

        internal void LoadingStarted()
        {
            this.Loading = true;
            this.Loaded = false;
            this.Error = null;
        }

        internal void LoadingDone()
        {
            this.Loading = false;
            this.Loaded = true;
            this.Error = null;
        }

        internal void LoadingError(CharacterError error)
        {
            this.Loading = false;
            this.Loaded = false;
            this.Error = error;
        }

        internal void LoadingFinished()
        {
            this.Loading = false;
        }
    }

    public class CData
    {
        public string? BestPercent { get; private set; } = null;

        public bool Cleared { get; private set; } = false;

        internal void SetCleared()
        {
            this.BestPercent = null;
            this.Cleared = true;
        }

        internal void SetBestPercent(string? bestPercent)
        {
            this.BestPercent = bestPercent;
            this.Cleared = false;
        }
    }

    public void StartLoading()
    {
        this.Status.LoadingStarted();
    }

    public void LoadCleared()
    {
        this.Status.LoadingDone();
        this.Data.SetCleared();
    }

    public void LoadBestPercent(string? bestPercent)
    {
        this.Status.LoadingDone();
        this.Data.SetBestPercent(bestPercent);
    }

    public void LoadError(CharacterError error)
    {
        this.Status.LoadingError(error);
    }

    public void FinishLoading()
    {
        this.Status.LoadingFinished();
    }
}
