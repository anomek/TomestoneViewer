using System.Collections.Generic;
using System.Numerics;

using Dalamud.Bindings.ImGui;
using TomestoneViewer.Character;
using TomestoneViewer.Character.Encounter;
using TomestoneViewer.Controller;
using TomestoneViewer.GUI.Widgets;

namespace TomestoneViewer.GUI.Main;

internal class PartyTableView : IWidget, Tabular.ITabularData
{
    private readonly WindowsController mainWindowController;

    private readonly Tabular table;

    private readonly Tooltiped header1 = new()
    {
        Main = new TextWidget()
        {
            Text = "<icon(82)><icon(83)><icon(84)>",
            Font = () => Service.Fonts.DefaultSmaller,
            Align = TextWidget.AlignType.Right,
            CompileSeString = true,
        },
        Tooltip = new TextWidget()
        {
            Font = () => Service.Fonts.TooltipDescription,
            Text = "Last Cleared\xe031 Clears Total<icon(117)>\nOn Selected Role",
            CompileSeString = true,
        },
    };

    private readonly Tooltiped header2 = new()
    {
        Main = new TextWidget()
        {
            Text = "<icon(87)>",
            Font = () => Service.Fonts.DefaultSmaller,
            Align = TextWidget.AlignType.Right,
            CompileSeString = true,
        },
        Tooltip = new TextWidget()
        {
            Font = () => Service.Fonts.TooltipDescription,
            Text = "Last Cleared\xe031 Clears Total<icon(117)>\nOn Any Role",
            CompileSeString = true,
        },
    };

    private List<Row> rows = [];

    public PartyTableView(WindowsController mainWindowController)
    {
        this.mainWindowController = mainWindowController;
        this.table = new Tabular(4)
        {
            Name = "MainWindowTablePartyView",
            TableFlags = ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.SizingFixedFit,
            CellPadding = new Vector2(0, 0),
            ItemSpacing = new Vector2(2, 0),
            Data = this,
        };
        this.table.Columns[0].SetStretch("nameplate");
        this.table.Columns[1].SetFixed("player track");
        this.table.Columns[2].SetFixed("status on role");
        this.table.Columns[3].SetFixed("status total");

        for (int i = 0; i < 8; i++)
        {
            this.rows.Add(new Row(this.mainWindowController));
        }
    }

    public Vector2 Draw()
    {
        if (Service.Configuration.FFLogsEnabled)
        {
            this.table.Columns[1].SetFixed("status on role");
        }
        else
        {
            this.table.Columns[1].SetStretch("disabled");
        }

        var members = Service.CharDataManager.PartyMembers;
        var location = Service.CharDataManager.CurrentEncounter;
        for (int i = 0; i < this.rows.Count; i++)
        {
            this.rows[i].Update(i < members.Count ? members[i] : null, location);
        }

        return this.table.Draw();
    }

    public IWidget? Get(int row, int column)
    {
        if (row == 0)
        {
            if (!Service.Configuration.FFLogsEnabled)
            {
                return null;
            }

            return column switch
            {
                2 => this.header1,
                3 => this.header2,
                _ => null,
            };
        }
        else if (row > this.rows.Count)
        {
            return null;
        }
        else
        {
            return this.rows[row - 1].Get(column);
        }
    }

    public int GetRowCount()
    {
        return Service.CharDataManager.PartyMembers.Count + 2;
    }

    public float GetMinWidth()
    {
        return 0;
    }

    public bool HasSeparatorBeforeRow(int row)
    {
        return row > 0;
    }

    private class Row
    {
        private readonly NameplateWidget nameplate = new();
        private readonly PlayerTrackWidget playerTrack = new();
        private readonly EncounterStatusView roleClears = new()
        {
            Total = false,
        };

        private readonly EncounterStatusView totalClears = new()
        {
            Total = true,
        };

        public Row(WindowsController mainWindowController)
        {
            this.nameplate.Callback = charData => mainWindowController.OpenCharacter(CharSelector.SelectById(charData.CharId));
        }

        public void Update(CharData? charData, Location location)
        {
            this.nameplate.CharData = charData;
            this.roleClears.EncounterData = charData?.EncounterData[Service.CharDataManager.CurrentEncounter];
            this.roleClears.CharData = charData;
            this.roleClears.BaseLine = this.nameplate.BaseLine;
            this.totalClears.EncounterData = charData?.EncounterData[Service.CharDataManager.CurrentEncounter];
            this.totalClears.CharData = charData;
            this.totalClears.BaseLine = this.nameplate.BaseLine;
            this.playerTrack.CharData = charData;
        }

        public IWidget? Get(int column)
        {
            return column switch
            {
                0 => this.nameplate,
                1 => this.playerTrack,
                2 => Service.Configuration.FFLogsEnabled ? this.roleClears : null,
                3 => this.totalClears,
                _ => null,
            };
        }
    }
}
