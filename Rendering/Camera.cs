using Avalonia;
using Lab6.Geometry;

namespace Lab6.Rendering;

public sealed class Camera
{
    public double Distance { get; set; } = 6.0;
    public double FieldOfView { get; set; } = 60.0;
    public ProjectionMode Projection { get; set; } = ProjectionMode.Perspective;

    public Avalonia.Point Project(Point3 worldPoint, double width, double height)
    {
        // Камера расположена в точке (0, 0, -Distance) и смотрит в направлении +Z.
        var viewPoint = new Point3(
            worldPoint.X,
            worldPoint.Y,
            worldPoint.Z + Distance);

        double projectedX;
        double projectedY;

        if (Projection == ProjectionMode.Perspective)
        {
            const double near = 0.1;

            if (viewPoint.Z <= near)
                viewPoint = new Point3(viewPoint.X, viewPoint.Y, near);

            double focal = height / (2 * Math.Tan(FieldOfView * Math.PI / 360));

            projectedX = focal * viewPoint.X / viewPoint.Z;
            projectedY = focal * viewPoint.Y / viewPoint.Z;
        }
        else
        {
            double angleY = Math.PI / 4;
            double angleX = Math.Asin(Math.Sqrt(1.0 / 3.0));

            var isometric = Matrix4.RotationY(angleY) * Matrix4.RotationX(angleX);
            var projected = isometric.TransformPoint(viewPoint);

            double scale = Math.Min(width, height) / 8.0;

            projectedX = projected.X * scale;
            projectedY = projected.Y * scale;
        }

        return new Avalonia.Point(
            width / 2 + projectedX,
            height / 2 - projectedY);
    }
}
