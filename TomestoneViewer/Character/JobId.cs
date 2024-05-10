using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomestoneViewer.Character;

public record JobId(uint Id)
{
    public static readonly JobId Empty = new(0);

    public override string ToString()
    {
        return this.Id.ToString();
    }
}
