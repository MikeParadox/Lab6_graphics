using Lab6.Geometry;

namespace Lab6.Builders;

public static class SurfaceBuilder
{
    public static Polyhedron Build(
        Func<double, double, double> function,
        double x0, double x1,
        double y0, double y1,
        int segments)
    {
        if (segments < 2)
            throw new ArgumentException("Количество разбиений должно быть не меньше двух.");

        var model = new Polyhedron();
        double dx = (x1 - x0) / segments;
        double dy = (y1 - y0) / segments;

        for (int iy = 0; iy <= segments; iy++)
        {
            for (int ix = 0; ix <= segments; ix++)
            {
                double x = x0 + ix * dx;
                double y = y0 + iy * dy;
                double z = function(x, y);

                model.Vertices.Add(new Point3(x, y, z));
            }
        }

        int rowLength = segments + 1;

        for (int iy = 0; iy < segments; iy++)
        {
            for (int ix = 0; ix < segments; ix++)
            {
                int a = iy * rowLength + ix;
                int b = a + 1;
                int c = a + rowLength + 1;
                int d = a + rowLength;

                model.Faces.Add(new Polygon(a, b, c, d));
            }
        }

        return model;
    }
}
