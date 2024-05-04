using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomestoneViewer.Character.TomestoneClient;

public record LodestoneId(uint Id)
{
    private readonly uint id = Id;

    public uint Id { get => this.id; }

    public override string ToString()
    {
        return this.id.ToString();
    }
}
