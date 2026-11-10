namespace Lab6.Geometry;

/// <summary>
/// Многогранник: список вершин и список граней.
/// </summary>
public sealed class Polyhedron
{
    public List<Point3> Vertices { get; } = new();
    public List<Polygon> Faces { get; } = new();

    public Point3 Center()
    {
        if (Vertices.Count == 0)
            return Point3.Zero;

        double x = 0, y = 0, z = 0;

        foreach (var vertex in Vertices)
        {
            x += vertex.X;
            y += vertex.Y;
            z += vertex.Z;
        }

        int count = Vertices.Count;
        return new Point3(x / count, y / count, z / count);
    }

    public void Apply(Matrix4 matrix)
    {
        for (int i = 0; i < Vertices.Count; i++)
            Vertices[i] = matrix.TransformPoint(Vertices[i]);
    }

    public Polyhedron Clone()
    {
        var copy = new Polyhedron();
        copy.Vertices.AddRange(Vertices);
        copy.Faces.AddRange(Faces.Select(face => new Polygon(face.VertexIndices)));
        return copy;
    }
}
