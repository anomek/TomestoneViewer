using TomestoneViewer.Character;
using TomestoneViewer.Character.Encounter;
using TomestoneViewer.GUI.Main;

namespace TomestoneViewer.Controller;

public class MainWindowController(CharacterSelectorController characterSelectorController)
{
    private readonly CharacterSelectorController characterSelectorController = characterSelectorController;

    private MainWindow mainWindow = null!;

    public void RegisterMainWindow(MainWindow mainWindow)
    {
        this.mainWindow = mainWindow;
    }

    public void Toggle()
    {
        this.mainWindow.Toggle();
        if (this.mainWindow.IsOpen && this.mainWindow.PartyView)
        {
            this.characterSelectorController.RefreshPartyData();
        }
    }

    public void OpenCharacter(CharSelector selector)
    {
        this.mainWindow.Open();
        this.mainWindow.PartyView = false;
        this.characterSelectorController.Select(selector);
    }

    public void OpenParty(Location? location = null)
    {
        this.mainWindow.Open();
        this.mainWindow.PartyView = true;
        this.characterSelectorController.RefreshPartyData(location);
    }

    public void TogglePartyView()
    {
        this.mainWindow.PartyView = !this.mainWindow.PartyView;
        if (this.mainWindow.PartyView)
        {
            this.characterSelectorController.RefreshPartyData();
        }
    }
}
