using System.Globalization;
using Lab6.Geometry;

namespace Lab6.IO;

public static class ObjImporter
{
    public static Polyhedron Load(string path)
    {
        using var stream = File.OpenRead(path);
        return Load(stream);
    }

    public static Polyhedron Load(Stream stream)
    {
        var model = new Polyhedron();

        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                continue;

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts[0] == "v" && parts.Length >= 4)
            {
                model.Vertices.Add(new Point3(
                    Parse(parts[1]),
                    Parse(parts[2]),
                    Parse(parts[3])));
            }
            else if (parts[0] == "f" && parts.Length >= 4)
            {
                var indices = new int[parts.Length - 1];

                for (int i = 1; i < parts.Length; i++)
                {
                    string token = parts[i].Split('/')[0];
                    int index = int.Parse(token, CultureInfo.InvariantCulture);

                    indices[i - 1] = index > 0
                        ? index - 1
                        : model.Vertices.Count + index;
                }

                model.Faces.Add(new Polygon(indices));
            }
        }

        if (model.Vertices.Count == 0)
            throw new InvalidDataException("В OBJ-файле не найдено вершин.");

        return model;
    }

    private static double Parse(string value) =>
        double.Parse(value, CultureInfo.InvariantCulture);
}
