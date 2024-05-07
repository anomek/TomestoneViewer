using System.Numerics;

namespace TomestoneViewer.Model;

public class Job
{
    private readonly string? abbreviation;

    public string Name { get; init; } = null!;

    public uint Id { get; init; }

    public Vector4 Color { get; init; }

    public string Abbreviation
    {
        get => this.abbreviation ?? this.Name;
        init => this.abbreviation = value;
    }
}
