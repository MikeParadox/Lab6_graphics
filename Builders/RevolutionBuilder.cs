using Lab6.Geometry;

namespace Lab6.Builders;

public static class RevolutionBuilder
{
    public static Polyhedron Build(
        IReadOnlyList<Point3> profile,
        string axis,
        int segments)
    {
        if (profile.Count < 2)
            throw new ArgumentException("Образующая должна содержать минимум две точки.");

        if (segments < 3)
            throw new ArgumentException("Количество разбиений должно быть не меньше трёх.");

        var model = new Polyhedron();
        double angleStep = 2 * Math.PI / segments;

        for (int ring = 0; ring <= segments; ring++)
        {
            double angle = ring * angleStep;
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);

            foreach (var point in profile)
            {
                var rotated = axis.Trim().ToUpperInvariant() switch
                {
                    "X" => new Point3(
                        point.X,
                        point.Y * cos - point.Z * sin,
                        point.Y * sin + point.Z * cos),

                    "Y" => new Point3(
                        point.X * cos + point.Z * sin,
                        point.Y,
                        -point.X * sin + point.Z * cos),

                    "Z" => new Point3(
                        point.X * cos - point.Y * sin,
                        point.X * sin + point.Y * cos,
                        point.Z),

                    _ => throw new ArgumentException("Ось должна быть X, Y или Z.")
                };

                model.Vertices.Add(rotated);
            }
        }

        int profileCount = profile.Count;

        for (int ring = 0; ring < segments; ring++)
        {
            int current = ring * profileCount;
            int next = (ring + 1) * profileCount;

            for (int i = 0; i < profileCount - 1; i++)
            {
                model.Faces.Add(new Polygon(
                    current + i,
                    current + i + 1,
                    next + i + 1,
                    next + i));
            }
        }

        return model;
    }
}
