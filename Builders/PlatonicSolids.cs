using Lab6.Geometry;

namespace Lab6.Builders;

public static class PlatonicSolids
{
    private static readonly double Phi = (1 + Math.Sqrt(5)) / 2;
    public static Polyhedron Tetrahedron()
    {
        var model = new Polyhedron();

        model.Vertices.AddRange(new[]
        {
            new Point3(1, 1, 1),
            new Point3(-1, -1, 1),
            new Point3(-1, 1, -1),
            new Point3(1, -1, -1)
        });

        model.Faces.AddRange(new[]
        {
            new Polygon(0, 1, 2),
            new Polygon(0, 3, 1),
            new Polygon(0, 2, 3),
            new Polygon(1, 3, 2)
        });

        return model;
    }

    public static Polyhedron Hexahedron(double size = 1)
    {
        var model = new Polyhedron();

        model.Vertices.AddRange(new[]
        {
            new Point3(-size, -size, -size),
            new Point3(size, -size, -size),
            new Point3(size, size, -size),
            new Point3(-size, size, -size),
            new Point3(-size, -size, size),
            new Point3(size, -size, size),
            new Point3(size, size, size),
            new Point3(-size, size, size)
        });

        model.Faces.AddRange(new[]
        {
            new Polygon(0, 1, 2, 3),
            new Polygon(4, 5, 6, 7),
            new Polygon(0, 1, 5, 4),
            new Polygon(2, 3, 7, 6),
            new Polygon(0, 3, 7, 4),
            new Polygon(1, 2, 6, 5)
        });

        return model;
    }

    public static Polyhedron Octahedron(double size = 1)
    {
        var model = new Polyhedron();

        model.Vertices.AddRange(new[]
        {
            new Point3(size, 0, 0),
            new Point3(-size, 0, 0),
            new Point3(0, size, 0),
            new Point3(0, -size, 0),
            new Point3(0, 0, size),
            new Point3(0, 0, -size)
        });

        model.Faces.AddRange(new[]
        {
            new Polygon(0, 2, 4), new Polygon(2, 1, 4),
            new Polygon(1, 3, 4), new Polygon(3, 0, 4),
            new Polygon(2, 0, 5), new Polygon(1, 2, 5),
            new Polygon(3, 1, 5), new Polygon(0, 3, 5)
        });

        return model;
    }

    public static Polyhedron Icosahedron(double size = 1)
    {
        double phi = Phi;
        
        double[][] rawVertices =
        {
            new[] { -1.0, phi, 0 }, new[] { 1.0, phi, 0 },
            new[] { -1.0, -phi, 0 }, new[] { 1.0, -phi, 0 },
            new[] { 0.0, -1, phi }, new[] { 0.0, 1, phi },
            new[] { 0.0, -1, -phi }, new[] { 0.0, 1, -phi },
            new[] { phi, 0, -1 }, new[] { phi, 0, 1 },
            new[] { -phi, 0, -1 }, new[] { -phi, 0, 1 }
        };

        var model = new Polyhedron();

        foreach (var raw in rawVertices)
        {
            var point = new Point3(raw[0], raw[1], raw[2]);
            model.Vertices.Add(point.Normalized() * size);
        }

        int[][] faces =
        {
            new[] { 0, 11, 5 }, new[] { 0, 5, 1 }, new[] { 0, 1, 7 },
            new[] { 0, 7, 10 }, new[] { 0, 10, 11 }, new[] { 1, 5, 9 },
            new[] { 5, 11, 4 }, new[] { 11, 10, 2 }, new[] { 10, 7, 6 },
            new[] { 7, 1, 8 }, new[] { 3, 9, 4 }, new[] { 3, 4, 2 },
            new[] { 3, 2, 6 }, new[] { 3, 6, 8 }, new[] { 3, 8, 9 },
            new[] { 4, 9, 5 }, new[] { 2, 4, 11 }, new[] { 6, 2, 10 },
            new[] { 8, 6, 7 }, new[] { 9, 8, 1 }
        };

        foreach (var face in faces)
            model.Faces.Add(new Polygon(face));

        return model;
    }

    
public static Polyhedron Dodecahedron(double size = 1)
{
    if (!double.IsFinite(size) || size <= 0)
        throw new ArgumentException("Size must be positive.");

    double phi = Phi;
    double invPhi = 1.0 / phi;

    var model = new Polyhedron();

    // =====================================================
    // PART 1/3: CREATE THE 20 VERTICES
    // =====================================================

    // Eight vertices of the cube: (+/-1, +/-1, +/-1).
    foreach (double x in new[] { -1.0, 1.0 })
    foreach (double y in new[] { -1.0, 1.0 })
    foreach (double z in new[] { -1.0, 1.0 })
    {
        model.Vertices.Add(new Point3(x, y, z));
    }

    // Four vertices: (0, +/-1/phi, +/-phi).
    foreach (double a in new[] { -1.0, 1.0 })
    foreach (double b in new[] { -1.0, 1.0 })
    {
        model.Vertices.Add(
            new Point3(0, a * invPhi, b * phi));
    }

    // Four vertices: (+/-1/phi, +/-phi, 0).
    foreach (double a in new[] { -1.0, 1.0 })
    foreach (double b in new[] { -1.0, 1.0 })
    {
        model.Vertices.Add(
            new Point3(a * invPhi, b * phi, 0));
    }

    // Four vertices: (+/-phi, 0, +/-1/phi).
    foreach (double a in new[] { -1.0, 1.0 })
    foreach (double b in new[] { -1.0, 1.0 })
    {
        model.Vertices.Add(
            new Point3(a * phi, 0, b * invPhi));
    }

    // All vertices have radius sqrt(3).
    // Scale the entire model uniformly so that its
    // circumradius equals the size parameter.
    double scale = size / Math.Sqrt(3);

    for (int i = 0; i < model.Vertices.Count; i++)
        model.Vertices[i] = model.Vertices[i] * scale;

    // =====================================================
    // PART 2/3: AUTOMATICALLY FIND THE 12 PENTAGONAL FACES
    // =====================================================

    const double epsilon = 1e-8;

    int count = model.Vertices.Count;

    var processedFaces = new HashSet<string>();

    Point3 center = model.Center();

    for (int i = 0; i < count; i++)
    for (int j = i + 1; j < count; j++)
    for (int k = j + 1; k < count; k++)
    {
        Point3 a = model.Vertices[i];
        Point3 b = model.Vertices[j];
        Point3 c = model.Vertices[k];

        // Calculate a normal to the candidate plane.
        Point3 normal = Point3.Cross(b - a, c - a);

        if (Point3.Dot(normal, normal) < epsilon * epsilon)
            continue;

        normal = normal.Normalized();

        bool positive = false;
        bool negative = false;

        // Check which side of the plane each vertex occupies.
        for (int t = 0; t < count; t++)
        {
            double distance = Point3.Dot(
                normal, model.Vertices[t] - a);

            if (distance > epsilon)
                positive = true;

            if (distance < -epsilon)
                negative = true;
        }

        // A supporting plane must have all vertices
        // on the same side or directly on the plane.
        if (positive && negative)
            continue;

        // Find all vertices belonging to this plane.
        var faceIndices = new List<int>();

        for (int t = 0; t < count; t++)
        {
            double distance = Point3.Dot(
                normal, model.Vertices[t] - a);

            if (Math.Abs(distance) < epsilon)
                faceIndices.Add(t);
        }

        // All dodecahedron faces are pentagons.
        if (faceIndices.Count != 5)
            continue;

        // Avoid duplicate faces.
        string key = string.Join(
            ",", faceIndices.OrderBy(index => index));

        if (!processedFaces.Add(key))
            continue;

        // Calculate the center of this pentagon.
        Point3 faceCenter = Point3.Zero;

        foreach (int index in faceIndices)
            faceCenter += model.Vertices[index];

        faceCenter = faceCenter * (1.0 / 5);

        // Determine the outward direction.
        Point3 outward = (faceCenter - center).Normalized();

        // Build two perpendicular directions in the face plane.
        Point3 reference = Math.Abs(outward.Z) < 0.9
            ? new Point3(0, 0, 1)
            : new Point3(0, 1, 0);

        Point3 u = Point3.Cross(reference, outward).Normalized();
        Point3 v = Point3.Cross(outward, u).Normalized();

        // Sort vertices around the face center.
        int[] orderedIndices = faceIndices
            .OrderBy(index =>
            {
                Point3 direction =
                    model.Vertices[index] - faceCenter;

                double x = Point3.Dot(direction, u);
                double y = Point3.Dot(direction, v);

                return Math.Atan2(y, x);
            })
            .ToArray();

        // Ensure outward-facing vertex winding.
        Point3 faceNormal = Point3.Cross(
            model.Vertices[orderedIndices[1]] -
            model.Vertices[orderedIndices[0]],

            model.Vertices[orderedIndices[2]] -
            model.Vertices[orderedIndices[0]]);

        if (Point3.Dot(faceNormal, outward) < 0)
            Array.Reverse(orderedIndices);

        model.Faces.Add(new Polygon(orderedIndices));
    }

    // =====================================================
    // PART 3/3: VERIFY THE GENERATED POLYHEDRON
    // =====================================================

    if (model.Vertices.Count != 20)
        throw new InvalidOperationException(
            "Dodecahedron must have 20 vertices.");

    if (model.Faces.Count != 12)
        throw new InvalidOperationException(
            $"Expected 12 faces, got {model.Faces.Count}.");

    if (model.Faces.Any(
        face => face.VertexIndices.Length != 5))
    {
        throw new InvalidOperationException(
            "Every face must have exactly 5 vertices.");
    }

    // Verify the 30 unique edges.
    var edges = new HashSet<(int, int)>();

    foreach (var face in model.Faces)
    {
        for (int i = 0; i < face.VertexIndices.Length; i++)
        {
            int a = face.VertexIndices[i];

            int b = face.VertexIndices[
                (i + 1) % face.VertexIndices.Length];

            edges.Add(a < b ? (a, b) : (b, a));
        }
    }

    if (edges.Count != 30)
        throw new InvalidOperationException(
            $"Expected 30 edges, got {edges.Count}.");

    return model;
}

}
