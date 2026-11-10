using System.Globalization;
using Lab6.Geometry;

namespace Lab6.IO;

public static class ObjExporter
{
    public static void Save(Polyhedron model, string path)
    {
        using var stream = File.Create(path);
        Save(model, stream);
    }

    public static void Save(Polyhedron model, Stream stream)
    {
        using var writer = new StreamWriter(stream, leaveOpen: true);

        writer.WriteLine("# Lab6 exported model");
        writer.WriteLine($"# vertices: {model.Vertices.Count}, faces: {model.Faces.Count}");

        foreach (var vertex in model.Vertices)
        {
            writer.WriteLine(FormattableString.Invariant(
                $"v {vertex.X:0.######} {vertex.Y:0.######} {vertex.Z:0.######}"));
        }

        foreach (var face in model.Faces)
        {
            writer.WriteLine("f " + string.Join(' ', face.VertexIndices.Select(index => index + 1)));
        }

        writer.Flush();
    }
}
