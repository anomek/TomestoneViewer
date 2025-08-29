using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dalamud.Bindings.ImGui;

namespace TomestoneViewer.Character.Client;

internal class SimpleParser(string source)
{
    private readonly string source = source;
    private int offset;


    internal bool Seek(string text)
    {
        return this.Find(text, string.Empty) != null;
    }

    internal string? Find(string begining, string terminus)
    {
        var start = this.source.IndexOf(begining, this.offset);
        if (start == -1)
        {
            return null;
        }

        start += begining.Length;

        var end = this.source.IndexOf(terminus, start);
        if (end == -1)
        {
            return null;
        }

        this.offset = end;
        return this.source.Substring(start, end - start);
    }
}
