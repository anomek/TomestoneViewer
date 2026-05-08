using System.Diagnostics.CodeAnalysis;

namespace TomestoneViewer.Character.Client;

internal class SimpleParser(string source)
{
    private readonly string source = source;
    private int offset;

    private delegate bool TryParse<T>([NotNullWhen(true)] string? s, out T result);

    internal bool Seek(string text)
    {
        return this.Find(text, string.Empty) != null;
    }

    internal long? FindLong(string begining, string terminus)
    {
        return this.FindAndParse<long>(begining, terminus, long.TryParse);
    }

    internal uint? FindUInt(string begining, string terminus)
    {
        return this.FindAndParse<uint>(begining, terminus, uint.TryParse);
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

    private T? FindAndParse<T>(string begining, string terminus, TryParse<T> tryParse)
    {
        var s = this.Find(begining, terminus);
        if (s == null)
        {
            return default;
        }
        else if (tryParse.Invoke(s, out var value))
        {
            return value;
        }
        else
        {
            return default;
        }
    }
}
