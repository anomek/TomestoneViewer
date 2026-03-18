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
    TomestoneLocationId LocationId)
{
    public record TomestoneCategory(
            string CategoryQueryParam,
            string ZoneQueryParam,
            string SummaryFieldName)
    {
        public static readonly TomestoneCategory ULTIMATE = new("ultimates", "ultimates", "ultimate");
        public static readonly TomestoneCategory SAVAGE = new("raids", "aac-heavyweight-savage", "savage");
    }
}
