using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomestoneViewer.GUI.Formatters;

public class TimeFormatters
{
    public static string FormatTimeRelativeShort(DateTimeOffset timestamp)
    {
        var diff = DateTimeOffset.Now - timestamp;
        if (diff.TotalDays < 1)
        {
            return "<1d";
        }
        else
        {
            var number = (int)diff.TotalDays;
            return $"{number}d";
        }
    }

    public static string FormatTimeRelative(DateTimeOffset timestamp)
    {
        var diff = DateTimeOffset.Now - timestamp;
        int number;
        string unit;
        if (diff.TotalMinutes < 120)
        {
            number = (int)diff.TotalMinutes;
            unit = "minute";
        }
        else if (diff.TotalHours < 48)
        {
            number = (int)diff.TotalHours;
            unit = "hour";
        }
        else if (diff.TotalDays < 65)
        {
            number = (int)diff.TotalDays;
            unit = "day";
        }
        else if (diff.TotalDays < 366)
        {
            number = (int)(diff.TotalDays / 30.43);
            unit = "month";
        }
        else
        {
            number = (int)(diff.TotalDays / 365.25);
            unit = "year";
        }

        var plular = number == 1 ? string.Empty : "s";
        var unitFull = $" {unit}{plular} ago";
        return $"{number}{unitFull}";
    }
}
