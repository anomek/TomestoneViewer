using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomestoneViewer.Character.Encounter;

/// <summary>
/// Server side encounter (zone) id
/// </summary>
public record EncounterId(int Id)
{
    public override string ToString()
    {
        return this.Id.ToString();
    }
}
