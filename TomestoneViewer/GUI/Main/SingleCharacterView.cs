using System.Collections.Generic;
using System.Numerics;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using TomestoneViewer.Character;
using TomestoneViewer.Character.Encounter;
using TomestoneViewer.Controller;
using TomestoneViewer.GUI.Widgets;

namespace TomestoneViewer.GUI.Main;

internal class SingleCharacterView
{
    private readonly Tabular table;
    private readonly TableData tableData;

    internal SingleCharacterView(WindowsController mainWindowController)
    {
        this.tableData = new(mainWindowController);
        this.table = new(2)
        {
            Name = "MainWindowTableSingleView",
            Data = this.tableData,
            CellPadding = new Vector2(0, 0),
            ItemSpacing = new Vector2(2, 0),
            TableFlags = ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit,
        };

        this.table.Columns[0].SetStretch("name");
        this.table.Columns[1].SetFixed("status");
    }

    public void Draw()
    {
        if (Service.CharDataManager.DisplayedChar == null || Service.CharDataManager.CharacterError != null)
        {
            return;
        }

        this.tableData.Update();
        this.table.Draw();
    }

    private class TableData : Tabular.ITabularData
    {
        private readonly List<IWidget?[]> widgets = [];
        private readonly List<int> categoryRows = [];
        private readonly List<int> emptyRows = [];
        private readonly Dictionary<Location, (EncounterplateWidget, EncounterStatusView, int)> statusViews = [];
        private readonly List<TextWidget> categories = [];
        private float baseRowHeight;

        internal TableData(WindowsController mainWindowController)
        {
            var rowCounter = 0;
            foreach (var category in Category.All())
            {
                if (rowCounter % 2 == 0)
                {
                    this.widgets.Add([null, null]);
                    this.emptyRows.Add(rowCounter);
                    rowCounter++;
                }

                var categoryWidget = new TextWidget
                {
                    Text = $" {category.DisplayName}",
                    Font = () => Service.Fonts.EncounterTypeHeader,
                };
                this.widgets.Add([categoryWidget, null]);
                this.categories.Add(categoryWidget);
                this.categoryRows.Add(rowCounter);

                rowCounter++;
                foreach (var location in category.Locations)
                {
                    var encounterplate = new EncounterplateWidget
                    {
                        Location = location,
                        Callback = mainWindowController.OpenParty,
                    };
                    var statusView = new EncounterStatusView()
                    {
                        Total = true,
                    };
                    this.statusViews[location] = (encounterplate, statusView, rowCounter);
                    this.widgets.Add([encounterplate, statusView]);
                    rowCounter++;
                }
            }

            this.widgets.Add([null, null]);
            this.emptyRows.Add(rowCounter);
        }

        public void Update()
        {
            this.baseRowHeight = this.RowBaseHeight() + GetRowPadding();
            var baseLine = (ImGui.GetFont().Ascent * ImGuiHelpers.GlobalScale) + GetRowPadding() + (1 * ImGuiHelpers.GlobalScale);
            var character = Service.CharDataManager.DisplayedChar;
            foreach (var location in Location.All())
            {
                var plate = this.statusViews[location].Item1;
                var view = this.statusViews[location].Item2;
                var rowNo = this.statusViews[location].Item3;
                plate.YOffset = this.GetRowExtraTopPadding(rowNo);
                plate.Height = this.baseRowHeight;
                plate.BaseLine = baseLine;
                view.YOffset = this.GetRowExtraTopPadding(rowNo);
                view.CharData = character;
                view.BaseLine = baseLine;
                view.EncounterData = character?.EncounterData[location];
            }

            foreach (var category in this.categories)
            {
                category.YOffset = GetRowPadding() + 1;
            }
        }

        public IWidget? Get(int row, int column)
        {
            if (column >= 2)
            {
                return null;
            }

            if (row >= this.widgets.Count)
            {
                return null;
            }

            return this.widgets[row][column];
        }

        public bool HasSeparatorBeforeRow(int row)
        {
            return this.categoryRows.Contains(row) || this.categoryRows.Contains(row - 1);
        }

        public int GetRowCount()
        {
            return this.widgets.Count;
        }

        public float? GetRowHeight(int row)
        {
            if (this.categoryRows.Contains(row) || this.emptyRows.Contains(row))
            {
                return null;
            }
            else
            {
                return this.baseRowHeight + this.GetRowExtraTopPadding(row);
            }
        }

        private static float GetRowPadding()
        {
            return 2 * ImGuiHelpers.GlobalScale;
        }

        private float RowBaseHeight()
        {
            ImGui.PushFont(UiBuilder.IconFont);
            var baseRowHeight = ImGui.CalcTextSize(FontAwesomeIcon.CheckCircle.ToIconString()).Y;
            var rowHeight = baseRowHeight + (GetRowPadding() * 2);
            ImGui.PopFont();
            return rowHeight;
        }

        private float GetRowExtraTopPadding(int row)
        {
            bool firstLocationRow = this.categoryRows.Contains(row - 1);
            return firstLocationRow ? 1 : 0;
        }
    }
}
