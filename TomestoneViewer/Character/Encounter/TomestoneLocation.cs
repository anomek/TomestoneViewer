using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomestoneViewer.Character.Encounter;

public record TomestoneLocation(
    TomestoneLocation.TomestoneCategory Category,
    string EncounterQueryParam,
    ExpansionQueryParam ExpansionQueryParam,
    UltimateId? UltimateId
)
{

    public record TomestoneCategory(
            string CategoryQueryParam,
            string ZoneQueryParam
        )
    {
        public static readonly TomestoneCategory ULTIMATE = new("ultimates", "ultimates");
        public static readonly TomestoneCategory SAVAGE = new("raids", "aac-cruiserweight-savage");
    }

}
