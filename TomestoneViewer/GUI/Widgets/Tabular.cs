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

namespace TomestoneViewer.GUI.Widgets;

public class Tabular : IWidget
{
    // Properties
    public required string Name { private get; init; }
    private int ColumnCount { get; init; }

    public required ITabularData Data { private get; init; }

    public Vector2? CellPadding { private get; init; }
    public Vector2? ItemSpacing { private get; init; }

    public ImGuiTableFlags TableFlags { private get; init; } = ImGuiTableFlags.None;

    // Edits

    public IList<Column> Columns { get; private set; } = [];


    public Tabular(int columnCount)
    {
        this.ColumnCount = columnCount;
        for (int i = 0; i < this.ColumnCount; i++)
        {
            Columns.Add(new Column());
        }
    }

    public Vector2 Draw()
    {
        var rowCount = this.Data.GetRowCount();
        if (rowCount == 0)
        {
            return default;
        }

        if (this.CellPadding != null)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, this.CellPadding.Value * ImGuiHelpers.GlobalScale);
        }

        if (this.ItemSpacing != null)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, this.ItemSpacing.Value * ImGuiHelpers.GlobalScale);
        }

        if (ImGui.BeginTable($"##{this.Name}", this.ColumnCount, this.TableFlags))
        {
            var i = 0;
            foreach (var column in this.Columns)
            {
                column.Setup(() => this.GetColumnWidth(i, rowCount));
                i++;
            }

            for (var row = 0; row < rowCount; row++)
            {
                ImGui.TableNextRow(this.Data.GetRowHeight(row).GetValueOrDefault());

                bool hasSeparator = this.Data.HasSeparatorBeforeRow(row);
                for (var column = 0; column < this.ColumnCount; column++)
                {
                    ImGui.TableNextColumn();
                    Util.ConditionalSeparator(hasSeparator);
                    this.Data.Get(row, column)?.Draw();
                }

            }

            ImGui.EndTable();
        }

        if (this.CellPadding != null)
        {
            ImGui.PopStyleVar();
        }

        if (this.ItemSpacing != null)
        {
            ImGui.PopStyleVar();
        }

        return ImGui.GetItemRectSize();
    }

    private float GetColumnWidth(int column, int rowsCount)
    {
        float maxWidth = 0;
        for (var i = 0; i < rowsCount; i++)
        {
            float? colWidth = Data.Get(i, column)?.GetMinWidth();
            maxWidth = float.Max(maxWidth, colWidth.GetValueOrDefault(0));
        }

        return maxWidth + (ImGui.GetStyle().CellPadding.X * 2);
    }

    public float GetMinWidth()
    {
        return 0;
    }

    public class Column
    {
        enum Type
        {
            Fixed, Streched
        }

        private string? name;
        private Type? type;


        public void SetStretch(string name)
        {
            this.name = name;
            this.type = Type.Streched;
        }

        public void SetFixed(string name)
        {
            this.name = name;
            this.type = Type.Fixed;
        }

        internal void Setup(Func<float> getMinWidth)
        {
            if (type != null && name != null)
            {
                if (type == Type.Fixed)
                {
                    ImGui.TableSetupColumn(name, ImGuiTableColumnFlags.WidthFixed, getMinWidth());
                }
                else if (type == Type.Streched)
                {
                    ImGui.TableSetupColumn(name, ImGuiTableColumnFlags.WidthStretch);
                }
            }
        }
    }


    public interface ITabularData
    {
        int GetRowCount();

        IWidget? Get(int row, int column);

        bool HasSeparatorBeforeRow(int row)
        {
            return false;
        }

        float? GetRowHeight(int row)
        {
            return null;
        }
    }
}
