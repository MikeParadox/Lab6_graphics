using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Lab6.Builders;
using Lab6.Geometry;
using Lab6.IO;
using Lab6.Rendering;

namespace Lab6.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private Polyhedron _model = PlatonicSolids.Hexahedron();
    private string _status = "Гексаэдр";
    private ProjectionMode _projection = ProjectionMode.Perspective;

    private double _translateX;
    private double _translateY;
    private double _translateZ;

    private double _rotationAngle = 30;
    private string _rotationAxis = "X";
    private double _scaleFactor = 1.2;
    private string _reflectionPlane = "XY";

    private double _axisPointX;
    private double _axisPointY;
    private double _axisPointZ;

    private double _axisDirectionX;
    private double _axisDirectionY;
    private double _axisDirectionZ = 1;

    private int _revolutionSegments = 24;
    private string _revolutionAxis = "Y";

    private int _surfaceSegments = 20;
    private string _surfaceFunction = "sin(x) * cos(y)";

    public event PropertyChangedEventHandler? PropertyChanged;

    public Camera Camera { get; } = new();

    public Polyhedron Model
    {
        get => _model;
        private set
        {
            _model = value;
            OnPropertyChanged();
        }
    }

    public string Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public ProjectionMode Projection
    {
        get => _projection;
        private set => SetProperty(ref _projection, value);
    }

    public double TranslateX { get => _translateX; set => SetProperty(ref _translateX, value); }
    public double TranslateY { get => _translateY; set => SetProperty(ref _translateY, value); }
    public double TranslateZ { get => _translateZ; set => SetProperty(ref _translateZ, value); }

    public double RotationAngle { get => _rotationAngle; set => SetProperty(ref _rotationAngle, value); }

    public string RotationAxis
    {
        get => _rotationAxis;
        set => SetProperty(ref _rotationAxis, value ?? "X");
    }

    public double ScaleFactor { get => _scaleFactor; set => SetProperty(ref _scaleFactor, value); }

    public string ReflectionPlane
    {
        get => _reflectionPlane;
        set => SetProperty(ref _reflectionPlane, value ?? "XY");
    }

    public double AxisPointX { get => _axisPointX; set => SetProperty(ref _axisPointX, value); }
    public double AxisPointY { get => _axisPointY; set => SetProperty(ref _axisPointY, value); }
    public double AxisPointZ { get => _axisPointZ; set => SetProperty(ref _axisPointZ, value); }

    public double AxisDirectionX { get => _axisDirectionX; set => SetProperty(ref _axisDirectionX, value); }
    public double AxisDirectionY { get => _axisDirectionY; set => SetProperty(ref _axisDirectionY, value); }
    public double AxisDirectionZ { get => _axisDirectionZ; set => SetProperty(ref _axisDirectionZ, value); }

    public int RevolutionSegments { get => _revolutionSegments; set => SetProperty(ref _revolutionSegments, value); }

    public string RevolutionAxis
    {
        get => _revolutionAxis;
        set => SetProperty(ref _revolutionAxis, value ?? "Y");
    }

    public int SurfaceSegments { get => _surfaceSegments; set => SetProperty(ref _surfaceSegments, value); }

    public string SurfaceFunction
    {
        get => _surfaceFunction;
        set => SetProperty(ref _surfaceFunction, value ?? string.Empty);
    }

    public ICommand CreateTetrahedronCommand { get; }
    public ICommand CreateHexahedronCommand { get; }
    public ICommand CreateOctahedronCommand { get; }
    public ICommand CreateIcosahedronCommand { get; }
    public ICommand CreateDodecahedronCommand { get; }

    public ICommand SetPerspectiveCommand { get; }
    public ICommand SetAxonometricCommand { get; }

    public ICommand ApplyTranslationCommand { get; }
    public ICommand ApplyRotationCommand { get; }
    public ICommand ApplyScaleCommand { get; }
    public ICommand ApplyReflectionCommand { get; }
    public ICommand ApplyCustomRotationCommand { get; }

    public ICommand BuildRevolutionCommand { get; }
    public ICommand BuildSurfaceCommand { get; }

    public AsyncRelayCommand LoadModelCommand { get; }
    public AsyncRelayCommand SaveModelCommand { get; }

    private Window? _ownerWindow;

    public MainViewModel()
    {
        CreateTetrahedronCommand = new RelayCommand(() => CreateModel(PlatonicSolids.Tetrahedron(), "Тетраэдр"));
        CreateHexahedronCommand = new RelayCommand(() => CreateModel(PlatonicSolids.Hexahedron(), "Гексаэдр"));
        CreateOctahedronCommand = new RelayCommand(() => CreateModel(PlatonicSolids.Octahedron(), "Октаэдр"));
        CreateIcosahedronCommand = new RelayCommand(() => CreateModel(PlatonicSolids.Icosahedron(), "Икосаэдр"));
        CreateDodecahedronCommand = new RelayCommand(() => CreateModel(PlatonicSolids.Dodecahedron(), "Додекаэдр"));

        SetPerspectiveCommand = new RelayCommand(() =>
        {
            Camera.Projection = ProjectionMode.Perspective;
            Projection = ProjectionMode.Perspective;
            Status = "Перспективная проекция";
        });

        SetAxonometricCommand = new RelayCommand(() =>
        {
            Camera.Projection = ProjectionMode.Axonometric;
            Projection = ProjectionMode.Axonometric;
            Status = "Аксонометрическая проекция";
        });

        ApplyTranslationCommand = new RelayCommand(ApplyTranslation);
        ApplyRotationCommand = new RelayCommand(ApplyRotationAroundCoordinateAxis);
        ApplyScaleCommand = new RelayCommand(ApplyScaleAroundCenter);
        ApplyReflectionCommand = new RelayCommand(ApplyReflection);
        ApplyCustomRotationCommand = new RelayCommand(ApplyRotationAroundCustomAxis);

        BuildRevolutionCommand = new RelayCommand(BuildRevolutionSolid);
        BuildSurfaceCommand = new RelayCommand(BuildSurface);

        LoadModelCommand = new AsyncRelayCommand(() => LoadModelAsync(_ownerWindow));
        SaveModelCommand = new AsyncRelayCommand(() => SaveModelAsync(_ownerWindow));
    }

    public void AttachWindow(Window owner) => _ownerWindow = owner;

    private void CreateModel(Polyhedron model, string name)
    {
        Model = model;
        Status = name;
    }

    public void ApplyTranslation()
    {
        Model.Apply(Matrix4.Translation(TranslateX, TranslateY, TranslateZ));
        Status = $"Смещение: ({TranslateX}; {TranslateY}; {TranslateZ})";
    }

    public void ApplyRotationAroundCoordinateAxis()
    {
        double radians = RotationAngle * Math.PI / 180;

        Matrix4 matrix = RotationAxis.ToUpperInvariant() switch
        {
            "X" => Matrix4.RotationX(radians),
            "Y" => Matrix4.RotationY(radians),
            "Z" => Matrix4.RotationZ(radians),
            _ => Matrix4.Identity
        };

        Model.Apply(matrix);
        Status = $"Поворот вокруг {RotationAxis} на {RotationAngle}°";
    }

    public void ApplyScaleAroundCenter()
    {
        var center = Model.Center();
        double factor = ScaleFactor;

        var matrix =
            Matrix4.Translation(-center.X, -center.Y, -center.Z) *
            Matrix4.Scale(factor, factor, factor) *
            Matrix4.Translation(center.X, center.Y, center.Z);

        Model.Apply(matrix);
        Status = $"Масштабирование относительно центра: {factor}";
    }

    public void ApplyReflection()
    {
        Model.Apply(Matrix4.Reflection(ReflectionPlane));
        Status = $"Отражение относительно плоскости {ReflectionPlane}";
    }

    public void ApplyRotationAroundCustomAxis()
    {
        try
        {
            var point = new Point3(AxisPointX, AxisPointY, AxisPointZ);
            var direction = new Point3(AxisDirectionX, AxisDirectionY, AxisDirectionZ);

            var matrix = Matrix4.RotationAroundLine(
                point,
                direction,
                RotationAngle * Math.PI / 180);

            Model.Apply(matrix);
            Status = $"Поворот вокруг произвольной прямой на {RotationAngle}°";
        }
        catch (Exception exception)
        {
            Status = "Ошибка: " + exception.Message;
        }
    }

    public void BuildRevolutionSolid()
    {
        var profile = new List<Point3>
        {
            new(0, -1.5, 0),
            new(1.0, -1.0, 0),
            new(0.8, 0, 0),
            new(1.3, 1.0, 0),
            new(0, 1.5, 0)
        };

        Model = RevolutionBuilder.Build(profile, RevolutionAxis, RevolutionSegments);
        Status = $"Фигура вращения: {RevolutionSegments} разбиений, ось {RevolutionAxis}";
    }

    public void BuildSurface()
    {
        try
        {
            var function = FunctionParser.Parse(SurfaceFunction);

            Model = SurfaceBuilder.Build(
                function,
                -3, 3,
                -3, 3,
                SurfaceSegments);

            Status = $"Поверхность z = {SurfaceFunction}";
        }
        catch (Exception exception)
        {
            Status = "Ошибка функции: " + exception.Message;
        }
    }

    public async Task LoadModelAsync(Window? owner)
    {
        if (owner is null)
            return;

        var file = await owner.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            Title = "Открыть OBJ-модель",
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Wavefront OBJ")
                {
                    Patterns = new[] { "*.obj" }
                }
            }
        });

        if (file.Count == 0)
            return;

        try
        {
            await using var stream = await file[0].OpenReadAsync();
            Model = ObjImporter.Load(stream);
            Status = $"Загружено: {file[0].Name}";
        }
        catch (Exception exception)
        {
            Status = "Ошибка загрузки: " + exception.Message;
        }
    }

    public async Task SaveModelAsync(Window? owner)
    {
        if (owner is null)
            return;

        var file = await owner.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Сохранить OBJ-модель",
            SuggestedFileName = "model.obj",
            DefaultExtension = "obj",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("Wavefront OBJ")
                {
                    Patterns = new[] { "*.obj" }
                }
            }
        });

        if (file is null)
            return;

        try
        {
            await using var stream = await file.OpenWriteAsync();
            ObjExporter.Save(Model, stream);
            Status = $"Сохранено: {file.Name}";
        }
        catch (Exception exception)
        {
            Status = "Ошибка сохранения: " + exception.Message;
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(name);
        return true;
    }
}
