namespace Lab6.Geometry;

/// <summary>
/// Матрица аффинного преобразования 4x4.
/// Точка представляется однородным вектором-строкой: p' = p * M.
/// </summary>
public readonly struct Matrix4
{
    public readonly double M11, M12, M13, M14;
    public readonly double M21, M22, M23, M24;
    public readonly double M31, M32, M33, M34;
    public readonly double M41, M42, M43, M44;

    public Matrix4(
        double m11, double m12, double m13, double m14,
        double m21, double m22, double m23, double m24,
        double m31, double m32, double m33, double m34,
        double m41, double m42, double m43, double m44)
    {
        M11 = m11; M12 = m12; M13 = m13; M14 = m14;
        M21 = m21; M22 = m22; M23 = m23; M24 = m24;
        M31 = m31; M32 = m32; M33 = m33; M34 = m34;
        M41 = m41; M42 = m42; M43 = m43; M44 = m44;
    }

    public static Matrix4 Identity => new(
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        0, 0, 0, 1);

    public static Matrix4 Translation(double tx, double ty, double tz) => new(
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        tx, ty, tz, 1);

    public static Matrix4 Scale(double sx, double sy, double sz) => new(
        sx, 0, 0, 0,
        0, sy, 0, 0,
        0, 0, sz, 0,
        0, 0, 0, 1);

    public static Matrix4 RotationX(double radians)
    {
        double c = Math.Cos(radians);
        double s = Math.Sin(radians);

        return new Matrix4(
            1, 0, 0, 0,
            0, c, s, 0,
            0, -s, c, 0,
            0, 0, 0, 1);
    }

    public static Matrix4 RotationY(double radians)
    {
        double c = Math.Cos(radians);
        double s = Math.Sin(radians);

        return new Matrix4(
            c, 0, -s, 0,
            0, 1, 0, 0,
            s, 0, c, 0,
            0, 0, 0, 1);
    }

    public static Matrix4 RotationZ(double radians)
    {
        double c = Math.Cos(radians);
        double s = Math.Sin(radians);

        return new Matrix4(
            c, s, 0, 0,
            -s, c, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);
    }

    /// <summary>
    /// Отражение относительно координатной плоскости.
    /// XY — отражение по Z, XZ — по Y, YZ — по X.
    /// </summary>
    public static Matrix4 Reflection(string plane) => plane.Trim().ToUpperInvariant() switch
    {
        "XY" => Scale(1, 1, -1),
        "XZ" => Scale(1, -1, 1),
        "YZ" => Scale(-1, 1, 1),
        _ => throw new ArgumentException("Плоскость должна быть XY, XZ или YZ.")
    };

    /// <summary>
    /// Поворот вокруг произвольной прямой, заданной точкой A и направлением.
    /// Реализуется как: T(-A) * Ry * Rx * Rz(angle) * Rx^-1 * Ry^-1 * T(A).
    /// </summary>
    public static Matrix4 RotationAroundLine(Point3 point, Point3 direction, double radians)
    {
        var d = direction.Normalized();

        if (d == Point3.Zero)
            throw new ArgumentException("Направляющий вектор прямой не может быть нулевым.");

        var toOrigin = Translation(-point.X, -point.Y, -point.Z);

        double yzLength = Math.Sqrt(d.Y * d.Y + d.Z * d.Z);
        Matrix4 alignYz;

        if (yzLength < 1e-12)
        {
            alignYz = Identity;
        }
        else
        {
            double cos = d.Z / yzLength;
            double sin = d.Y / yzLength;

            alignYz = new Matrix4(
                1, 0, 0, 0,
                0, cos, sin, 0,
                0, -sin, cos, 0,
                0, 0, 0, 1);
        }

        double length = Math.Sqrt(d.X * d.X + yzLength * yzLength);
        Matrix4 alignXz;

        if (length < 1e-12)
        {
            alignXz = Identity;
        }
        else
        {
            double cos = yzLength / length;
            double sin = -d.X / length;

            alignXz = new Matrix4(
                cos, 0, -sin, 0,
                0, 1, 0, 0,
                sin, 0, cos, 0,
                0, 0, 0, 1);
        }

        return
            toOrigin *
            alignYz *
            alignXz *
            RotationZ(radians) *
            alignXz.Inverse() *
            alignYz.Inverse() *
            toOrigin.Inverse();
    }

    public Matrix4 Inverse()
    {
        double a = M11, b = M12, c = M13;
        double d = M21, e = M22, f = M23;
        double g = M31, h = M32, i = M33;

        double determinant =
            a * (e * i - f * h) -
            b * (d * i - f * g) +
            c * (d * h - e * g);

        if (Math.Abs(determinant) < 1e-12)
            throw new InvalidOperationException("Матрица необратима.");

        double k = 1.0 / determinant;

        var rotationInverse = new Matrix4(
            (e * i - f * h) * k, -(b * i - c * h) * k, (b * f - c * e) * k, 0,
            -(d * i - f * g) * k, (a * i - c * g) * k, -(a * f - c * d) * k, 0,
            (d * h - e * g) * k, -(a * h - b * g) * k, (a * e - b * d) * k, 0,
            0, 0, 0, 1);

        var translation = new Point3(M41, M42, M43);
        var inverseTranslation = rotationInverse.TransformPoint(translation.Negated());

        return new Matrix4(
            rotationInverse.M11, rotationInverse.M12, rotationInverse.M13, 0,
            rotationInverse.M21, rotationInverse.M22, rotationInverse.M23, 0,
            rotationInverse.M31, rotationInverse.M32, rotationInverse.M33, 0,
            inverseTranslation.X, inverseTranslation.Y, inverseTranslation.Z, 1);
    }

    public static Matrix4 operator *(Matrix4 a, Matrix4 b) => new(
        a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31 + a.M14 * b.M41,
        a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32 + a.M14 * b.M42,
        a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33 + a.M14 * b.M43,
        a.M11 * b.M14 + a.M12 * b.M24 + a.M13 * b.M34 + a.M14 * b.M44,

        a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31 + a.M24 * b.M41,
        a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32 + a.M24 * b.M42,
        a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33 + a.M24 * b.M43,
        a.M21 * b.M14 + a.M22 * b.M24 + a.M23 * b.M34 + a.M24 * b.M44,

        a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31 + a.M34 * b.M41,
        a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32 + a.M34 * b.M42,
        a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33 + a.M34 * b.M43,
        a.M31 * b.M14 + a.M32 * b.M24 + a.M33 * b.M34 + a.M34 * b.M44,

        a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + a.M44 * b.M41,
        a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + a.M44 * b.M42,
        a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + a.M44 * b.M43,
        a.M41 * b.M14 + a.M42 * b.M24 + a.M43 * b.M34 + a.M44 * b.M44);

    public Point3 TransformPoint(Point3 point)
    {
        double x = point.X * M11 + point.Y * M21 + point.Z * M31 + M41;
        double y = point.X * M12 + point.Y * M22 + point.Z * M32 + M42;
        double z = point.X * M13 + point.Y * M23 + point.Z * M33 + M43;
        double w = point.X * M14 + point.Y * M24 + point.Z * M34 + M44;

        return Math.Abs(w) < 1e-12
            ? new Point3(x, y, z)
            : new Point3(x / w, y / w, z / w);
    }
}
