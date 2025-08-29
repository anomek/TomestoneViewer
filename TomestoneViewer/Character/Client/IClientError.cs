using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomestoneViewer.Character.Encounter;

namespace TomestoneViewer.Character.Client;

public interface IClientError : IEncounterDataError
{
    bool Cachable { get; }
}
