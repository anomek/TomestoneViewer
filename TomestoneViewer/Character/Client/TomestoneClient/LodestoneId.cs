using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomestoneViewer.Character.Client.TomestoneClient;

public record LodestoneId(uint Id)
{
    public override string ToString()
    {
        return this.Id.ToString();
    }
}
