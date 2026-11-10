namespace Lab6.Geometry;

/// <summary>
/// Точка (вершина) в трёхмерном пространстве.
/// </summary>
public readonly record struct Point3(double X, double Y, double Z)
{
    public static Point3 Zero => new(0, 0, 0);

    public static Point3 operator +(Point3 a, Point3 b) =>
        new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static Point3 operator -(Point3 a, Point3 b) =>
        new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static Point3 operator *(Point3 a, double k) =>
        new(a.X * k, a.Y * k, a.Z * k);

    public static double Distance(Point3 a, Point3 b)
    {
        var d = a - b;
        return Math.Sqrt(d.X * d.X + d.Y * d.Y + d.Z * d.Z);
    }

    public static Point3 Cross(Point3 a, Point3 b) => new(
        a.Y * b.Z - a.Z * b.Y,
        a.Z * b.X - a.X * b.Z,
        a.X * b.Y - a.Y * b.X);

    public static double Dot(Point3 a, Point3 b) =>
        a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    public Point3 Normalized()
    {
        double length = Math.Sqrt(X * X + Y * Y + Z * Z);
        return length < 1e-12 ? Zero : new Point3(X / length, Y / length, Z / length);
    }

    public Point3 Negated() => new(-X, -Y, -Z);

    public override string ToString() => $"({X:0.###}; {Y:0.###}; {Z:0.###})";
}
