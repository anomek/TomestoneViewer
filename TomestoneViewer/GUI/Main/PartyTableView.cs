using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomestoneViewer.Character;
using TomestoneViewer.Character.Encounter;
using TomestoneViewer.Controller;
using TomestoneViewer.GUI.Widgets;
using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using TomestoneViewer.Character;
using TomestoneViewer.Character.Encounter;
using TomestoneViewer.Controller;
using TomestoneViewer.GUI.Formatters;
using TomestoneViewer.GUI.Widgets;

namespace TomestoneViewer.GUI.Main;

internal class PartyTableView : IWidget, Tabular.ITabularData
{

    private readonly MainWindowController mainWindowController;

    private readonly Func<bool> ffLogsEnabled;

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

    private class Row
    {
        private readonly Func<bool> ffLogsEnabled;
        private readonly NameplateWidget nameplate = new();
        private readonly EncounterStatusView roleClears = new()
        {
            Total = false,
        };

        private readonly EncounterStatusView totalClears = new()
        {
            Total = true,
        };

        public Row(MainWindowController mainWindowController, Func<bool> ffLogsEnabled)
        {
            this.ffLogsEnabled = ffLogsEnabled;
            totalClears.ffLogsEnabled = ffLogsEnabled;
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
        }

        public IWidget? Get(int column)
        {
            return column switch
            {
                0 => this.nameplate,
                1 => this.ffLogsEnabled() ? this.roleClears : null,
                2 => this.totalClears,
                _ => null,
            };
        }
    }

    public PartyTableView(MainWindowController mainWindowController, Func<bool> ffLogsEnabled)
    {
        this.ffLogsEnabled = ffLogsEnabled;
        this.mainWindowController = mainWindowController;
        this.table = new Tabular(3)
        {
            Name = "MainWindowTablePartyView",
            TableFlags = ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.SizingFixedFit,
            CellPadding = new Vector2(0, 0),
            ItemSpacing = new Vector2(2, 0),
            Data = this,
        };
        this.table.Columns[0].SetStretch("nameplate");
        this.table.Columns[1].SetFixed("status on role");
        this.table.Columns[2].SetFixed("status total");

        for (int i = 0; i < 8; i++)
        {
            this.rows.Add(new Row(this.mainWindowController, this.ffLogsEnabled));
        }
    }

    public Vector2 Draw()
    {
        if (ffLogsEnabled())
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
            if (!ffLogsEnabled())
            {
                return null;
            }

            return column switch
            {
                1 => header1,
                2 => header2,
                _ => null,
            };
        }
        else if (row > rows.Count)
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
}
