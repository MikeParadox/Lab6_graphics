namespace Lab6.Geometry;

/// <summary>
/// Многоугольник (грань) многогранника.
/// Хранит индексы вершин в массиве вершин многогранника.
/// </summary>
public sealed class Polygon
{
    public int[] VertexIndices { get; }

    public Polygon(params int[] vertexIndices)
    {
        if (vertexIndices.Length < 3)
            throw new ArgumentException("Грань должна содержать минимум три вершины.");

        VertexIndices = vertexIndices.ToArray();
    }

    public Point3 Normal(IReadOnlyList<Point3> vertices)
    {
        var a = vertices[VertexIndices[0]];
        var b = vertices[VertexIndices[1]];
        var c = vertices[VertexIndices[2]];

        return Point3.Cross(b - a, c - a).Normalized();
    }

    public Point3 Center(IReadOnlyList<Point3> vertices)
    {
        double x = 0, y = 0, z = 0;

        foreach (int index in VertexIndices)
        {
            var vertex = vertices[index];
            x += vertex.X;
            y += vertex.Y;
            z += vertex.Z;
        }

        int count = VertexIndices.Length;
        return new Point3(x / count, y / count, z / count);
    }
}
