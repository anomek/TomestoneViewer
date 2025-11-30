using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomestoneViewer.GUI.Widgets;
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
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace TomestoneViewer.GUI.Main;

internal class SingleCharacterView
{

    private readonly Tabular table;
    private readonly TableData tableData;

    internal SingleCharacterView(WindowsController mainWindowController, Func<bool> ffLogsEnabled)
    {
        this.tableData = new(mainWindowController, ffLogsEnabled);
        this.table = new(2)
        {
            Name = "MainWindowTableSingleView",
            Data = tableData,
            CellPadding = new Vector2(0, 0),
            ItemSpacing = new Vector2(2, 0),
            TableFlags = ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit,
        };

        this.table.Columns[0].SetStretch("name");
        this.table.Columns[1].SetFixed("status");
    }

    class TableData : Tabular.ITabularData
    {
        private readonly List<IWidget?[]> widgets = [];
        private readonly List<int> categoryRows = [];
        private readonly List<int> emptyRows = [];
        private readonly Dictionary<Location, (EncounterplateWidget, EncounterStatusView, int)> statusViews = [];
        private readonly List<TextWidget> categories = [];
        private float baseRowHeight;
        internal TableData(WindowsController mainWindowController, Func<bool> ffLogsEnabled)
        {

            var rowCounter = 0;
            foreach (var category in Category.All())
            {
                if (rowCounter % 2 == 0)
                {
                    widgets.Add([null, null]);
                    emptyRows.Add(rowCounter);
                    rowCounter++;
                }

                var categoryWidget = new TextWidget
                {
                    Text = $" {category.DisplayName}",
                    Font = () => Service.Fonts.EncounterTypeHeader,
                };
                widgets.Add([categoryWidget, null]);
                categories.Add(categoryWidget);
                categoryRows.Add(rowCounter);

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
                        ffLogsEnabled = ffLogsEnabled,
                    };
                    statusViews[location] = (encounterplate, statusView, rowCounter);
                    widgets.Add([encounterplate, statusView]);
                    rowCounter++;
                }
            }
            widgets.Add([null, null]);
            emptyRows.Add(rowCounter);
        }

        public void Update()
        {
            this.baseRowHeight = RowBaseHeight();
            var baseLine = baseRowHeight - ImGui.GetFont().Ascent + 1;
            var character = Service.CharDataManager.DisplayedChar;
            foreach (var location in Location.All())
            {
                var plate = statusViews[location].Item1;
                var view = statusViews[location].Item2;
                var rowNo = statusViews[location].Item3;
                plate.YOffset = GetRowExtraTopPadding(rowNo);
                plate.Height = this.baseRowHeight;
                plate.BaseLine = baseLine;
                view.YOffset = GetRowExtraTopPadding(rowNo);
                view.CharData = character;
                view.BaseLine = baseLine;
                view.EncounterData = character?.EncounterData[location];    
            }
            foreach (var category in categories)
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
            if (row >= widgets.Count)
            {
                return null;
            }
            return widgets[row][column];

        }

        public bool HasSeparatorBeforeRow(int row)
        {
            return categoryRows.Contains(row) || categoryRows.Contains(row - 1);
        }

        public int GetRowCount()
        {
            return widgets.Count;
        }


        public float? GetRowHeight(int row)
        {
            if (categoryRows.Contains(row) || emptyRows.Contains(row))
            {
                return null;
            }
            else
            {
                return this.baseRowHeight + GetRowExtraTopPadding(row);
            }
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

        private float GetRowPadding()
        {
            return 2 * ImGuiHelpers.GlobalScale;
        }
    }

    public void Draw()
    {
        if (Service.CharDataManager.DisplayedChar == null || Service.CharDataManager.CharacterError != null)
        {
            return;
        }
        this.tableData.Update();
        this.table.Draw();
        if (true) return;

        ImGui.PushFont(UiBuilder.IconFont);
        var rowPadding = 2 * ImGuiHelpers.GlobalScale;
        var baseRowHeight = ImGui.CalcTextSize(FontAwesomeIcon.CheckCircle.ToIconString()).Y;
        var rowHeight = baseRowHeight + (rowPadding * 2);
        ImGui.PopFont();

        var character = Service.CharDataManager.DisplayedChar;

        var firstRow = true;
        foreach (var category in Category.All())
        {
            // fake row to start to ensure dark background
            ImGui.TableNextRow();


            // category row
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Separator();

            Service.Fonts.EncounterTypeHeader.Push();
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + rowPadding + 1);
            ImGui.TextUnformatted($" {category.DisplayName}");
            Service.Fonts.EncounterTypeHeader.Pop();

            ImGui.TableNextColumn();
            ImGui.Separator();

            firstRow = false;
            var counter = 0;
            foreach (var location in category.Locations)
            {
                var rowExtraTopPadding = counter == 0 ? 1 : 0;
                ImGui.TableNextRow(rowHeight + rowExtraTopPadding);
                ImGui.TableNextColumn();
                Util.ConditionalSeparator(counter == 0);
                var rowTopYPos = ImGui.GetCursorPosY();

                ImGui.SetCursorPosY(rowTopYPos + rowExtraTopPadding);
                if (ImGui.Selectable($"  ##EncounterListSelectable{location.DisplayName}", false, ImGuiSelectableFlags.None, new Vector2(0, rowHeight)))
                {
                    // this.mainWindowController.OpenParty(location);
                }

                ImGui.SameLine();
                ImGui.SetCursorPosY(rowTopYPos + rowExtraTopPadding + (rowHeight - ImGui.GetTextLineHeight()) / 2);
                ImGui.TextUnformatted(location.DisplayName);

                ImGui.TableNextColumn();
                var cellStart = ImGui.GetCursorPos() + new Vector2(0, rowExtraTopPadding);
                Util.ConditionalSeparator(counter == 0);
                ImGui.SetCursorPosY(rowTopYPos + rowPadding + rowExtraTopPadding);
                counter++;
            }
        }

        ImGui.EndTable();
    }

    // there must be better way to do that lol
    public IWidget? Get(int row, int column)
    {

        return null;
    }

    public int GetRowCount()
    {
        var categories = Category.All();
        var locations = categories
            .SelectMany(c => c.Locations)
            .Count();
        return locations + categories.Count();
    }
}
