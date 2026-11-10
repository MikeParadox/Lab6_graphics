using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Media;
using Lab6.Geometry;

namespace Lab6.Rendering;

public static class Renderer
{
    // Each model gets a fixed initial view.
    // Applying transformations does not recalculate it.
    private sealed class InitialView
    {
        public Point3 Center { get; }
        public double Radius { get; }

        public InitialView(Polyhedron model)
        {
            Center = model.Center();

            double radius = model.Vertices.Max(
                p => Point3.Distance(Center, p));

            Radius = radius > 1e-12 ? radius : 1;
        }
    }

    private static readonly ConditionalWeakTable<Polyhedron, InitialView>
        Views = new();

    public static void Draw(
        Polyhedron model,
        Camera camera,
        DrawingContext context,
        Size size)
    {
        if (size.Width <= 2 || size.Height <= 2 ||
            model.Vertices.Count == 0 ||
            model.Faces.Count == 0)
        {
            return;
        }

        if (model.Vertices.Any(p =>
                !double.IsFinite(p.X) ||
                !double.IsFinite(p.Y) ||
                !double.IsFinite(p.Z)))
        {
            return;
        }

        var view = Views.GetValue(
            model, m => new InitialView(m));

        // This uses the ORIGINAL center and radius, not the current ones.
        // Consequently, translation and scaling remain visible.
        var vertices = model.Vertices
            .Select(p => (p - view.Center) * (1.0 / view.Radius))
            .ToArray();

        var rotation =
            Matrix4.RotationY(Math.PI / 4) *
            Matrix4.RotationX(Math.Asin(Math.Sqrt(1.0 / 3.0)));

        bool perspective =
            camera.Projection == ProjectionMode.Perspective;

        var projected = new Point[vertices.Length];
        var depths = new double[vertices.Length];

        double pixelsPerUnit =
            Math.Min(size.Width, size.Height) * 0.35;

        const double distance = 4;

        for (int i = 0; i < vertices.Length; i++)
        {
            var p = rotation.TransformPoint(vertices[i]);

            depths[i] = p.Z;

            double factor = perspective
                ? distance / Math.Max(0.1, distance + p.Z)
                : 1;

            projected[i] = new Point(
                size.Width / 2 + p.X * pixelsPerUnit * factor,
                size.Height / 2 - p.Y * pixelsPerUnit * factor);
        }

        var faces = model.Faces
            .Where(face =>
                face.VertexIndices.Length >= 3 &&
                face.VertexIndices.All(index =>
                    index >= 0 && index < vertices.Length))
            .OrderByDescending(face =>
                face.VertexIndices.Average(index => depths[index]));

        var light = new Point3(0.4, 0.8, -1).Normalized();

        using (context.PushClip(
                   new Rect(0, 0, size.Width, size.Height)))
        {
            foreach (var face in faces)
            {
                var geometry = new StreamGeometry();

                using (var stream = geometry.Open())
                {
                    stream.BeginFigure(
                        projected[face.VertexIndices[0]], true);

                    for (int i = 1;
                         i < face.VertexIndices.Length;
                         i++)
                    {
                        stream.LineTo(
                            projected[face.VertexIndices[i]]);
                    }

                    stream.EndFigure(true);
                }

                var normal = face.Normal(vertices);

                double intensity = Math.Clamp(
                    0.35 + 0.65 *
                    Math.Abs(Point3.Dot(normal, light)),
                    0, 1);

                context.DrawGeometry(
                    new SolidColorBrush(Color.FromRgb(
                        (byte)(60 + 160 * intensity),
                        (byte)(50 + 140 * intensity),
                        (byte)(40 + 110 * intensity))),
                    new Pen(Brushes.DarkSlateGray, 1.1),
                    geometry);
            }
        }
    }

    // Optional: call this to fit an existing model again.
    public static void ResetView(Polyhedron model)
    {
        Views.Remove(model);
    }
}