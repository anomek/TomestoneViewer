using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomestoneViewer.Character.Encounter;

public record ExpansionQueryParam(string Value)
{
    public static readonly ExpansionQueryParam STB = new("stormblood");
    public static readonly ExpansionQueryParam SHB = new("shadowbringers");
    public static readonly ExpansionQueryParam EW = new("endwalker");

    public override string ToString()
    {
        return this.Value;
    }
}
